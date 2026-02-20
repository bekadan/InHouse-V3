namespace InHouse.BuildingBlocks.Domain;

public abstract class AggregateRoot<TId> : Entity<TId>, IHasDomainEvents
    where TId : notnull
{
    protected AggregateRoot(TId id) : base(id) { }

    // EF Core için
    protected AggregateRoot() { }

    protected static IReadOnlyCollection<IDomainEvent> CollectDomainEvents(params IHasDomainEvents[] sources)
    {
        var events = new List<IDomainEvent>(capacity: 8);
        foreach (var src in sources)
        {
            if (src.DomainEvents.Count == 0) continue;
            events.AddRange(src.DomainEvents);
        }

        return events;
    }
}
