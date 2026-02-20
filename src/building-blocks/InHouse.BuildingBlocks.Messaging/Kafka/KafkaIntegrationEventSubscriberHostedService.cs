using Confluent.Kafka;
using InHouse.BuildingBlocks.Abstractions.Integration.Bus;
using InHouse.BuildingBlocks.Abstractions.Integration.Consumers;
using InHouse.BuildingBlocks.Abstractions.Integration.Events;
using InHouse.BuildingBlocks.Abstractions.Integration.Versioning;
using InHouse.BuildingBlocks.Abstractions.Projection;
using InHouse.BuildingBlocks.Api.Integration.Consumers; // IntegrationEventConsumerRunner burada diye varsayıyorum
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Text;

namespace InHouse.BuildingBlocks.Messaging.Kafka;

public sealed class KafkaIntegrationEventSubscriberHostedService : BackgroundService
{
    private static readonly ActivitySource ActivitySource = new("InHouse.Messaging.Kafka");

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly KafkaOptions _options;
    private readonly ILogger<KafkaIntegrationEventSubscriberHostedService> _logger;

    public KafkaIntegrationEventSubscriberHostedService(
        IServiceScopeFactory scopeFactory,
        IOptions<KafkaOptions> options,
        ILogger<KafkaIntegrationEventSubscriberHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = _options.BootstrapServers,
            GroupId = _options.ConsumerGroupId,
            EnableAutoCommit = _options.EnableAutoCommit,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            SessionTimeoutMs = _options.SessionTimeoutMs,
            MaxPollIntervalMs = _options.MaxPollIntervalMs,
            EnablePartitionEof = false
        };

        using var consumer = new ConsumerBuilder<string, byte[]>(config)
            .SetErrorHandler((_, e) => _logger.LogError("Kafka error: {Reason}", e.Reason))
            .Build();

        consumer.Subscribe(_options.Topic);

        _logger.LogInformation("Kafka subscriber started. Topic={Topic} GroupId={GroupId}",
            _options.Topic, _options.ConsumerGroupId);

        while (!stoppingToken.IsCancellationRequested)
        {
            ConsumeResult<string, byte[]>? cr = null;

            try
            {
                cr = consumer.Consume(_options.PollInterval);
                if (cr is null) continue;

                using var activity = ActivitySource.StartActivity("kafka.consume", ActivityKind.Consumer);
                activity?.SetTag("messaging.system", "kafka");
                activity?.SetTag("messaging.destination", _options.Topic);
                activity?.SetTag("kafka.partition", cr.Partition.Value);
                activity?.SetTag("kafka.offset", cr.Offset.Value);

                await ProcessMessageAsync(cr, consumer, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kafka consume loop error.");

                // Eğer burada exception aldıysak commit etmiyoruz => retry
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
        }

        try
        {
            consumer.Close();
        }
        catch { /* ignore */ }

        _logger.LogInformation("Kafka subscriber stopped.");
    }

    private async Task ProcessMessageAsync(
        ConsumeResult<string, byte[]> cr,
        IConsumer<string, byte[]> consumer,
        CancellationToken ct)
    {
        // Her mesajda scoped DI: handler seti + runner + serializer
        using var scope = _scopeFactory.CreateScope();

        var serializer = scope.ServiceProvider.GetRequiredService<IIntegrationEventEnvelopeSerializer>();
        var upcasting = scope.ServiceProvider.GetRequiredService<IEventUpcastingPipeline>();
        var runner = scope.ServiceProvider.GetRequiredService<IntegrationEventConsumerRunner>();
        var dlq = scope.ServiceProvider.GetRequiredService<KafkaDlqPublisher>();

        var envelope = serializer.Deserialize(cr.Message.Value);
        envelope = upcasting.UpcastToLatest(envelope);

        var handlers = scope.ServiceProvider
            .GetServices<IIntegrationEventHandler>()
            .Where(h => h.EventType == envelope.EventType && h.EventVersion == envelope.EventVersion)
            .ToArray();

        var projections = scope.ServiceProvider
            .GetServices<IProjectionHandler>()
            .Where(p => p.EventType == envelope.EventType && p.EventVersion == envelope.EventVersion)
            .ToArray();

        // Headers'tan attempt sayısını okuyalım
        var attempt = ReadIntHeader(cr.Message.Headers, "x-attempt") ?? 0;

        // routing: eventType + version match
        var matched = handlers.Where(h => h.EventType == envelope.EventType && h.EventVersion == envelope.EventVersion).ToArray();
        if (matched.Length == 0)
        {
            _logger.LogWarning("No handler for EventType={EventType} v{Version}. MessageId={MessageId}. Move to DLQ.",
                envelope.EventType, envelope.EventVersion, envelope.MessageId);
            
            await MoveToDlqAndCommitAsync(cr, consumer, dlq, attempt, ct);
            return;
        }

        try
        {
            // Aynı event birden fazla handler'a gidebilir (projection + side effect vs)
            foreach (var handler in handlers)
                await runner.ConsumeAsync(handler, envelope, ct);

            foreach (var projection in projections)
                await projection.ProjectAsync(envelope, ct);

            // Success => commit
            consumer.Commit(cr);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Handler failed. MessageId={MessageId}, EventType={EventType}, Attempt={Attempt}",
                envelope.MessageId, envelope.EventType, attempt);

            attempt++;

            if (attempt >= _options.MaxPoisonAttempts)
            {
                _logger.LogError("Poison threshold reached. Moving message to DLQ. MessageId={MessageId}", envelope.MessageId);
                await MoveToDlqAndCommitAsync(cr, consumer, dlq, attempt, ct);
                return;
            }

            // retry: commit etmiyoruz => Kafka aynı offset'i tekrar verir.
            // (Inbox idempotency + handler safety => duplicate execution safe)
            // Ancak attempt bilgisini Kafka'da update edemeyiz (immutable). DLQ kararını log/metrics üzerinden takip ederiz.
        }
    }

    private async Task MoveToDlqAndCommitAsync(
        ConsumeResult<string, byte[]> cr,
        IConsumer<string, byte[]> consumer,
        KafkaDlqPublisher dlq,
        int attempt,
        CancellationToken ct)
    {
        var headers = CloneHeaders(cr.Message.Headers);
        UpsertHeader(headers, "x-attempt", attempt.ToString());
        UpsertHeader(headers, "x-dlq-reason", "poison-or-unhandled");

        await dlq.PublishAsync(cr.Message.Key ?? string.Empty, cr.Message.Value, headers, ct);

        // Commit ederek partition'ı unblock ediyoruz
        consumer.Commit(cr);
    }

    private static int? ReadIntHeader(Headers headers, string key)
    {
        var val = headers.LastOrDefault(h => h.Key == key);
        if (val is null) return null;

        var s = Encoding.UTF8.GetString(val.GetValueBytes());
        return int.TryParse(s, out var i) ? i : null;
    }

    private static Headers CloneHeaders(Headers original)
    {
        var headers = new Headers();
        foreach (var h in original)
            headers.Add(h.Key, h.GetValueBytes());
        return headers;
    }

    private static void UpsertHeader(Headers headers, string key, string value)
    {
        // Confluent headers allow duplicates; remove old then add new
        var toRemove = headers.Where(h => h.Key == key).ToArray();
        foreach (var h in toRemove)
            headers.Remove(key);

        headers.Add(key, Encoding.UTF8.GetBytes(value));
    }
}