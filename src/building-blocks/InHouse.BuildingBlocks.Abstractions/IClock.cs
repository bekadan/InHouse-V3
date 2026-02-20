namespace InHouse.BuildingBlocks.Abstractions;

public interface IClock
{
    DateTime UtcNow { get; }
}
