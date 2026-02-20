using InHouse.BuildingBlocks.Api.Http;
using Microsoft.AspNetCore.Http;

namespace InHouse.BuildingBlocks.Api.Middleware;

public sealed class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(RequestDelegate next) => _next = next;

    public async Task Invoke(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(InHouseHeaders.TenantId, out var tenant) &&
            !string.IsNullOrWhiteSpace(tenant.ToString()))
        {
            context.Items[InHouseHeaders.TenantId] = tenant.ToString().Trim();
        }

        await _next(context);
    }
}
