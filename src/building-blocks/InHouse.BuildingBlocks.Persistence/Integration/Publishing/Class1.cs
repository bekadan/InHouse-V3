using InHouse.BuildingBlocks.Abstractions.Messaging;
using InHouse.BuildingBlocks.Persistence.Outbox;
using InHouse.BuildingBlocks.Persistence.Tenancy;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

public sealed class EfCoreIntegrationEventPublisher : IIntegrationEventPublisher
{
    private readonly DbContext _db;
    private readonly ITenantProvider _tenantProvider;

    public EfCoreIntegrationEventPublisher(
        DbContext db,
        ITenantProvider tenantProvider)
    {
        _db = db;
        _tenantProvider = tenantProvider;
    }

    public async Task PublishAsync(
        IIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.GetTenantId();

        var message = new OutboxMessage
        {
            MessageId = Guid.NewGuid(),
            TenantId = tenantId,
            EventType = integrationEvent.EventType,
            EventVersion = integrationEvent.EventVersion,
            OccurredOnUtc = DateTime.UtcNow,
            PayloadJson = JsonSerializer.Serialize(integrationEvent)
        };

        _db.Set<OutboxMessage>().Add(message);
        await _db.SaveChangesAsync(cancellationToken);
    }
}