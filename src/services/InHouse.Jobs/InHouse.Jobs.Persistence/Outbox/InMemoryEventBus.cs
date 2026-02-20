using InHouse.BuildingBlocks.Abstractions;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace InHouse.Jobs.Persistence.Outbox;

public sealed class InMemoryEventBus : IEventBus
{
    private readonly ILogger<InMemoryEventBus> _logger;

    public InMemoryEventBus(ILogger<InMemoryEventBus> logger)
        => _logger = logger;

    public Task PublishAsync(string eventType, string payloadJson, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Publishing event {EventType}: {Payload}", eventType, payloadJson);
        return Task.CompletedTask;
    }
}