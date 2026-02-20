using InHouse.BuildingBlocks.Abstractions.Messaging;
using InHouse.Jobs.Persistence.Outbox;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace InHouse.Jobs.Persistence.Outbox;

public sealed class EfOutboxIntegrationEventPublisher : IIntegrationEventPublisher
{
    private readonly JobsDbContext _db;

    public EfOutboxIntegrationEventPublisher(JobsDbContext db) => _db = db;

    public Task PublishAsync(IIntegrationEvent @event, CancellationToken cancellationToken = default)
    {
        var payload = JsonSerializer.Serialize(@event, @event.GetType(), new JsonSerializerOptions(JsonSerializerDefaults.Web));

        _db.OutboxMessages.Add(new OutboxMessage(
            id: Guid.CreateVersion7(),
            occurredOnUtc: @event.OccurredOnUtc,
            type: @event.EventType,
            payloadJson: payload));

        // 🔥 ÖNEMLİ: SaveChanges burada yapmıyoruz.
        // Command zaten SaveChanges yapacak. Böylece outbox message aynı transaction’da commit olur.
        return Task.CompletedTask;
    }
}