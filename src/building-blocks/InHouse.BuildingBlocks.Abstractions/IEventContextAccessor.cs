namespace InHouse.BuildingBlocks.Abstractions;

public interface IEventContextAccessor
{
    EventContext Current { get; }
}
