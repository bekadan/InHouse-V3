using InHouse.BuildingBlocks.Abstractions.Integration.Events;
using InHouse.BuildingBlocks.Abstractions.Messaging;
using InHouse.BuildingBlocks.Persistence.Outbox;
using InHouse.BuildingBlocks.Persistence.Tenancy;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace InHouse.BuildingBlocks.Persistence.Integration.Publishing;

public sealed class EfCoreIntegrationEventPublisher : IIntegrationEventPublisher
{
    private readonly DbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;

    public EfCoreIntegrationEventPublisher(DbContext dbContext, ITenantProvider tenantProvider)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _tenantProvider = tenantProvider ?? throw new ArgumentNullException(nameof(tenantProvider));
    }

    public Task PublishAsync(IIntegrationEvent @event, CancellationToken cancellationToken = default)
    {
        if (@event is null)
            throw new ArgumentNullException(nameof(@event));

        var tenantId = _tenantProvider.TenantId;

        var eventType =
            @event is IHasIntegrationEventType typed ? typed.EventType : @event.GetType().Name;

        var eventVersion = (@event as IVersionedIntegrationEvent)?.Version ?? 1;
        var payloadJson = JsonSerializer.Serialize(@event, @event.GetType());

        // ✅ OutboxMessage: new + set yok. Factory kullan.
        var outbox = OutboxMessage.Create(
            tenantId: tenantId,
            eventType: eventType,
            eventVersion: eventVersion,
            occurredOnUtc: DateTime.UtcNow,
            payloadJson: payloadJson);

        _dbContext.Set<OutboxMessage>().Add(outbox);

        // IMPORTANT: SaveChanges burada çağrılmayabilir (komut transaction’ı içinde zaten çağrılıyor olabilir).
        // Repoda event yazma SaveChanges interceptor/behavior ile yapılıyorsa burada çağırma.
        return Task.CompletedTask;
    }
}