using Confluent.Kafka;
using InHouse.BuildingBlocks.Abstractions.Integration.Bus;
using InHouse.BuildingBlocks.Abstractions.Integration.Events;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Reflection.PortableExecutable;
using System.Text;

namespace InHouse.BuildingBlocks.Messaging.Kafka;

public sealed class KafkaMessageBusPublisher : IMessageBusPublisher, IDisposable
{
    private static readonly ActivitySource ActivitySource = new("InHouse.Messaging.Kafka");

    private readonly IProducer<string, byte[]> _producer;
    private readonly KafkaOptions _options;
    private readonly IIntegrationEventEnvelopeSerializer _serializer;
    private readonly ILogger<KafkaMessageBusPublisher> _logger;

    public KafkaMessageBusPublisher(
        IOptions<KafkaOptions> options,
        IIntegrationEventEnvelopeSerializer serializer,
        ILogger<KafkaMessageBusPublisher> logger)
    {
        _options = options.Value;
        _serializer = serializer;
        _logger = logger;

        var config = new ProducerConfig
        {
            BootstrapServers = _options.BootstrapServers,
            EnableIdempotence = _options.EnableIdempotence,
            MessageTimeoutMs = _options.MessageTimeoutMs,
            Acks = Acks.All,
            LingerMs = 5
        };

        _producer = new ProducerBuilder<string, byte[]>(config).Build();
    }

    public async Task PublishAsync(IntegrationEventEnvelope envelope, CancellationToken ct)
    {
        using var activity = ActivitySource.StartActivity("kafka.produce", ActivityKind.Producer);
        activity?.SetTag("messaging.system", "kafka");
        activity?.SetTag("messaging.destination", _options.Topic);
        activity?.SetTag("messaging.message_id", envelope.MessageId);
        activity?.SetTag("tenant.id", envelope.TenantId);
        activity?.SetTag("messaging.event_type", envelope.EventType);
        activity?.SetTag("messaging.event_version", envelope.EventVersion);

        var key = envelope.TenantId; // ordering per tenant (safe default)
        var value = _serializer.Serialize(envelope);

        var headers = new Headers
        {
            { "message-id", Encoding.UTF8.GetBytes(envelope.MessageId) },
            { "tenant-id", Encoding.UTF8.GetBytes(envelope.TenantId) },
            { "event-type", Encoding.UTF8.GetBytes(envelope.EventType) },
            { "event-version", Encoding.UTF8.GetBytes(envelope.EventVersion.ToString()) }
        };

        if (envelope.Headers is not null)
        {
            foreach (var kv in envelope.Headers)
                headers.Add(kv.Key, Encoding.UTF8.GetBytes(kv.Value));
        }

        try
        {
            var result = await _producer.ProduceAsync(
                _options.Topic,
                new Message<string, byte[]> { Key = key, Value = value, Headers = headers },
                ct);

            activity?.SetTag("kafka.partition", result.Partition.Value);
            activity?.SetTag("kafka.offset", result.Offset.Value);
        }
        catch (ProduceException<string, byte[]> ex)
        {
            _logger.LogError(ex, "Kafka produce failed for MessageId={MessageId}", envelope.MessageId);
            throw;
        }
    }

    public void Dispose()
    {
        try { _producer.Flush(TimeSpan.FromSeconds(5)); } catch { /* ignore */ }
        _producer.Dispose();
    }
}