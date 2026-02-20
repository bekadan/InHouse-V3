using InHouse.BuildingBlocks.Persistence.Tenancy;

namespace InHouse.Jobs.Api.Middleware;

public sealed class TenantRequiredMiddleware
{
    private readonly RequestDelegate _next;

    private static readonly string[] _excludedPaths =
    [
        "/health",
        "/metrics",
        "/swagger"
    ];

    public TenantRequiredMiddleware(RequestDelegate next)
        => _next = next;

    public async Task InvokeAsync(HttpContext context, ITenantProvider tenant)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        if (_excludedPaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
        {
            await _next(context);
            return;
        }

        if (tenant.TenantId is null)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new
            {
                title = "Tenant header is required",
                detail = "X-Tenant-Id header must contain a valid CompanyId (Guid)."
            });
            return;
        }

        await _next(context);
    }
}