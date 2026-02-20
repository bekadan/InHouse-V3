namespace InHouse.BuildingBlocks.Persistence.Tenancy;

public interface ITenantProvider
{
    Guid? TenantId { get; }
    bool BypassTenantFilter { get; }
    IDisposable BeginBypassScope();
}