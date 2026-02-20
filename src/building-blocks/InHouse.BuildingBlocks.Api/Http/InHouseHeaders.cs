namespace InHouse.BuildingBlocks.Api.Http;

public static class InHouseHeaders
{
    public const string CorrelationId = "X-Correlation-Id";
    public const string RequestId = "X-Request-Id";
    public const string TenantId = "X-Tenant-Id";
    public const string Source = "X-Source"; // UI/API/Job/Worker vs.
}
