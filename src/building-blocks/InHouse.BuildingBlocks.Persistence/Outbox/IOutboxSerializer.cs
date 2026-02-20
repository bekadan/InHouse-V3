namespace InHouse.BuildingBlocks.Persistence.Outbox;

public interface IOutboxSerializer
{
    string Serialize(object value);
}
