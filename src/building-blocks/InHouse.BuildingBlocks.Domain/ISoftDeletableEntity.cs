namespace InHouse.BuildingBlocks.Domain;

public interface ISoftDeletableEntity
{
    bool IsDeleted { get; }
    DateTime? DeletedOnUtc { get; }

    void SoftDelete(DateTime utcNow);
}
