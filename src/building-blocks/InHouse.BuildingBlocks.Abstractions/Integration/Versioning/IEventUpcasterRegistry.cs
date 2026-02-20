namespace InHouse.BuildingBlocks.Abstractions.Integration.Versioning;

public interface IEventUpcasterRegistry
{
    IEventUpcaster? Find(string eventType, int fromVersion);
    int GetLatestVersion(string eventType);
}