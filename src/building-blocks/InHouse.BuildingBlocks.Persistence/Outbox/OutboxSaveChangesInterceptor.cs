using InHouse.BuildingBlocks.Abstractions;
using InHouse.BuildingBlocks.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace InHouse.BuildingBlocks.Persistence.Outbox;

public sealed class OutboxSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly IEventContextAccessor _eventContextAccessor;
    private readonly IOutboxSerializer _serializer;
    private readonly IClock _clock;

    public OutboxSaveChangesInterceptor(
        IEventContextAccessor eventContextAccessor,
        IOutboxSerializer serializer,
        IClock clock)
    {
        _eventContextAccessor = eventContextAccessor;
        _serializer = serializer;
        _clock = clock;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
            EnqueueOutboxMessages(eventData.Context);

        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
            EnqueueOutboxMessages(eventData.Context);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void EnqueueOutboxMessages(DbContext db)
    {
        // Domain events olan tracked entry’leri bul
        var aggregates = db.ChangeTracker.Entries()
            .Select(e => e.Entity)
            .OfType<IHasDomainEvents>()
            .ToList();

        if (aggregates.Count == 0)
            return;

        var now = _clock.UtcNow;
        var ctx = _eventContextAccessor.Current;

        var outboxSet = db.Set<OutboxMessage>();
        foreach (var aggregate in aggregates)
        {
            var events = aggregate.DequeueDomainEvents();
            if (events.Count == 0) continue;

            foreach (var ev in events)
            {
                // Event envelope (çok basit): headersJson içinde context
                var headers = new Dictionary<string, object?>
                {
                    ["correlationId"] = ctx.CorrelationId,
                    ["tenantId"] = ctx.TenantId,
                    ["actorId"] = ctx.ActorId,
                    ["source"] = ctx.Source,
                    ["requestId"] = ctx.RequestId,
                    ["eventId"] = ev.EventId,
                    ["occurredOnUtc"] = ev.OccurredOnUtc
                };

                var type = ev.GetType().FullName ?? ev.GetType().Name;

                var payloadJson = _serializer.Serialize(ev);
                var headersJson = _serializer.Serialize(headers);

                outboxSet.Add(new OutboxMessage(
                    id: Guid.CreateVersion7(),
                    occurredOnUtc: ev.OccurredOnUtc,
                    type: type,
                    payloadJson: payloadJson,
                    headersJson: headersJson,
                    createdOnUtc: now));
            }
        }
    }
}
