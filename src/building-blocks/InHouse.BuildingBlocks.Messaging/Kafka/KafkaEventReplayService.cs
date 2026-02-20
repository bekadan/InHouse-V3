using System.Text;
using Confluent.Kafka;
using InHouse.BuildingBlocks.Abstractions.Integration.Bus;
using InHouse.BuildingBlocks.Abstractions.Integration.Replay;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace InHouse.BuildingBlocks.Messaging.Kafka;

public sealed class KafkaEventReplayService : IEventReplayService
{
    private readonly KafkaOptions _options;
    private readonly IIntegrationEventEnvelopeSerializer _serializer;
    private readonly IMessageBusPublisher _publisher;
    private readonly ILogger<KafkaEventReplayService> _logger;

    public KafkaEventReplayService(
        IOptions<KafkaOptions> options,
        IIntegrationEventEnvelopeSerializer serializer,
        IMessageBusPublisher publisher,
        ILogger<KafkaEventReplayService> logger)
    {
        _options = options.Value;
        _serializer = serializer;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task<EventReplayResult> ReplayFromDlqAsync(EventReplayRequest request, CancellationToken ct)
    {
        var replayId = Guid.NewGuid();
        var scanned = 0;
        var republished = 0;
        var skipped = 0;

        var config = new ConsumerConfig
        {
            BootstrapServers = _options.BootstrapServers,
            GroupId = $"{_options.ConsumerGroupId}.replay.{replayId}",
            EnableAutoCommit = false,
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        using var consumer = new ConsumerBuilder<string, byte[]>(config).Build();
        consumer.Subscribe(_options.DlqTopic);

        _logger.LogWarning(
            "Starting DLQ replay. ReplayId={ReplayId} TenantId={TenantId} ForceReprocess={Force} Filters(EventType={EventType}, Version={Version}, From={From}, To={To})",
            replayId, request.TenantId, request.ForceReprocess, request.EventType, request.EventVersion, request.OccurredFromUtc, request.OccurredToUtc);

        // Safety: bounded scan window (avoid endless replay jobs)
        var emptyPolls = 0;
        const int maxEmptyPolls = 5;

        while (!ct.IsCancellationRequested)
        {
            var cr = consumer.Consume(TimeSpan.FromSeconds(1));
            if (cr is null)
            {
                emptyPolls++;
                if (emptyPolls >= maxEmptyPolls) break;
                continue;
            }

            emptyPolls = 0;
            scanned++;

            var envelope = _serializer.Deserialize(cr.Message.Value);

            if (!MatchesFilter(envelope, request))
            {
                skipped++;
                consumer.Commit(cr);
                continue;
            }

            // Replay headers
            var headers = envelope.Headers is null
                ? new Dictionary<string, string>(StringComparer.Ordinal)
                : new Dictionary<string, string>(envelope.Headers, StringComparer.Ordinal);

            headers["x-replay"] = "true";
            headers["x-replay-id"] = replayId.ToString();
            headers["x-replay-requested-by"] = request.RequestedBy;
            headers["x-replay-reason"] = request.Reason;
            headers["x-replay-force"] = request.ForceReprocess ? "true" : "false";

            var replayEnvelope = envelope with { Headers = headers };

            // Republish to main topic via publisher abstraction (uses KafkaMessageBusPublisher under the hood)
            await _publisher.PublishAsync(replayEnvelope, ct);
            republished++;

            // commit dlq offset so replay job is repeatable without duplicating scan
            consumer.Commit(cr);
        }

        _logger.LogWarning(
            "DLQ replay finished. ReplayId={ReplayId} Scanned={Scanned} Republished={Republished} Skipped={Skipped}",
            replayId, scanned, republished, skipped);

        return new EventReplayResult(replayId, scanned, republished, skipped);
    }

    private static bool MatchesFilter(InHouse.BuildingBlocks.Abstractions.Integration.Events.IntegrationEventEnvelope env, EventReplayRequest req)
    {
        if (!StringComparer.Ordinal.Equals(env.TenantId, req.TenantId))
            return false;

        if (req.EventType is not null && !StringComparer.Ordinal.Equals(env.EventType, req.EventType))
            return false;

        if (req.EventVersion is not null && env.EventVersion != req.EventVersion.Value)
            return false;

        if (req.OccurredFromUtc is not null && env.OccurredOnUtc < req.OccurredFromUtc.Value)
            return false;

        if (req.OccurredToUtc is not null && env.OccurredOnUtc > req.OccurredToUtc.Value)
            return false;

        return true;
    }
}