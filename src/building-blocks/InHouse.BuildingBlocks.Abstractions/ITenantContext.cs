namespace InHouse.BuildingBlocks.Abstractions;

public interface ITenantContext
{
    string? TenantId { get; }
    bool HasTenant => !string.IsNullOrWhiteSpace(TenantId);
}
