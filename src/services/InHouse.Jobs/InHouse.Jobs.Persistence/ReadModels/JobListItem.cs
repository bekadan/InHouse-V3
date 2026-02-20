using System;

namespace InHouse.BuildingBlocks.Persistence.ReadModels;

public sealed class JobListItem
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid CompanyId { get; set; }
    public string Title { get; set; } = default!;
    public string EmploymentType { get; set; } = default!;
    public string Status { get; set; } = default!;
    public DateTime CreatedOnUtc { get; set; }

    // FTS
    public string SearchVector { get; set; } = default!;

    // pgvector
    public float[]? Embedding { get; set; }
}
