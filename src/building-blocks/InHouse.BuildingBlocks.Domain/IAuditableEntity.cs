namespace InHouse.BuildingBlocks.Domain;

public interface IAuditableEntity
{
    DateTime CreatedOnUtc { get; }
    string? CreatedBy { get; }

    DateTime? ModifiedOnUtc { get; }
    string? ModifiedBy { get; }

    void SetCreated(DateTime utcNow, string? actorId);
    void SetModified(DateTime utcNow, string? actorId);
}