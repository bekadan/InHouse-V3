using System.Text;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace InHouse.BuildingBlocks.Messaging.Kafka;

public sealed class KafkaDlqPublisher : IDisposable
{
    private readonly IProducer<string, byte[]> _producer;
    private readonly KafkaOptions _options;
    private readonly ILogger<KafkaDlqPublisher> _logger;

    public KafkaDlqPublisher(IOptions<KafkaOptions> options, ILogger<KafkaDlqPublisher> logger)
    {
        _options = options.Value;
        _logger = logger;

        var config = new ProducerConfig
        {
            BootstrapServers = _options.BootstrapServers,
            EnableIdempotence = true,
            Acks = Acks.All
        };

        _producer = new ProducerBuilder<string, byte[]>(config).Build();
    }

    public async Task PublishAsync(
        string key,
        byte[] value,
        Headers headers,
        CancellationToken ct)
    {
        await _producer.ProduceAsync(
            _options.DlqTopic,
            new Message<string, byte[]> { Key = key, Value = value, Headers = headers },
            ct);

        _logger.LogWarning("Message moved to DLQ topic {DlqTopic}", _options.DlqTopic);
    }

    public void Dispose()
    {
        try { _producer.Flush(TimeSpan.FromSeconds(5)); } catch { }
        _producer.Dispose();
    }
}