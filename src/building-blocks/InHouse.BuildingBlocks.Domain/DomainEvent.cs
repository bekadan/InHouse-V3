namespace InHouse.BuildingBlocks.Domain;

public abstract record DomainEvent : IDomainEvent
{
    protected DomainEvent(DateTime occurredOnUtc)
    {
        EventId = Guid.CreateVersion7();
        OccurredOnUtc = occurredOnUtc;
    }

    public Guid EventId { get; }
    public DateTime OccurredOnUtc { get; }
}
