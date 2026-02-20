namespace InHouse.BuildingBlocks.Domain;

public interface IDomainEvent
{
    Guid EventId { get; }
    DateTime OccurredOnUtc { get; }
}
