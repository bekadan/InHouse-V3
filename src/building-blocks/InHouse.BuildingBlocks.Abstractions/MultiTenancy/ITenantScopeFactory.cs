namespace InHouse.BuildingBlocks.Abstractions.MultiTenancy;

public interface ITenantScopeFactory
{
    IDisposable BeginScope(Guid tenantId);
}