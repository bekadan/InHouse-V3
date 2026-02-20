namespace InHouse.BuildingBlocks.Domain;

public static class DomainEventsExtensions
{
    public static IReadOnlyCollection<IDomainEvent> DequeueDomainEvents(this IHasDomainEvents source)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (source.DomainEvents.Count == 0)
            return Array.Empty<IDomainEvent>();

        var events = source.DomainEvents.ToArray();
        source.ClearDomainEvents();
        return events;
    }
}
