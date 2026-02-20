using Microsoft.Extensions.Logging;

namespace InHouse.BuildingBlocks.Messaging;

public sealed class InMemoryEventBus : IEventBus
{
    private readonly ILogger<InMemoryEventBus> _logger;

    public InMemoryEventBus(ILogger<InMemoryEventBus> logger)
        => _logger = logger;

    public Task PublishAsync(EventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("InMemory publish: {EventType} - {MessageId}",
            envelope.EventType, envelope.MessageId);

        return Task.CompletedTask;
    }
}
