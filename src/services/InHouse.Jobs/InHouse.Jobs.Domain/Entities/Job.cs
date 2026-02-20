using InHouse.BuildingBlocks.Domain;

namespace InHouse.Jobs.Domain.Entities;

public sealed class Job : IAuditableEntity, ISoftDeletableEntity
{
    public Guid Id { get; private set; }
    public Guid CompanyId { get; private set; }

    public string Title { get; private set; } = default!;
    public string Status { get; private set; } = default!;
    public DateTime CreatedOnUtc { get; private set; }

    // Audit
    public string? CreatedBy { get; private set; }
    public DateTime? ModifiedOnUtc { get; private set; }
    public string? ModifiedBy { get; private set; }

    // Soft delete
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedOnUtc { get; private set; }

    public byte[] RowVersion { get; private set; } = default!;

    private Job() { } // EF

    public Job(Guid id, Guid companyId, string title, string? actorId = null)
    {
        Id = id;
        CompanyId = companyId;
        Title = title;
        Status = "Draft";

        SetCreated(DateTime.UtcNow, actorId);
    }

    public void Publish()
    {
        if (IsDeleted) throw new InvalidOperationException("Cannot publish a deleted job.");
        Status = "Published";
    }

    public void SoftDelete(DateTime utcNow)
    {
        if (IsDeleted) return;
        IsDeleted = true;
        DeletedOnUtc = utcNow;
    }

    public void SetCreated(DateTime utcNow, string? actorId)
    {
        CreatedOnUtc = utcNow;
        CreatedBy = actorId;
        ModifiedOnUtc = null;
        ModifiedBy = null;
    }

    public void SetModified(DateTime utcNow, string? actorId)
    {
        ModifiedOnUtc = utcNow;
        ModifiedBy = actorId;
    }
}