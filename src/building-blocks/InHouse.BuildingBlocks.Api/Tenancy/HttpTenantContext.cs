using InHouse.BuildingBlocks.Abstractions;
using InHouse.BuildingBlocks.Api.Http;
using Microsoft.AspNetCore.Http;

namespace InHouse.BuildingBlocks.Api.Tenancy;

public sealed class HttpTenantContext : ITenantContext
{
    private readonly IHttpContextAccessor _http;

    public HttpTenantContext(IHttpContextAccessor http) => _http = http;

    public string? TenantId
    {
        get
        {
            var ctx = _http.HttpContext;
            if (ctx is null) return null;

            // Middleware set etmediyse direkt request header’dan da okuyalım
            if (ctx.Items.TryGetValue(InHouseHeaders.TenantId, out var v) && v is string s && !string.IsNullOrWhiteSpace(s))
                return s;

            if (ctx.Request.Headers.TryGetValue(InHouseHeaders.TenantId, out var header) && !string.IsNullOrWhiteSpace(header.ToString()))
                return header.ToString().Trim();

            return null;
        }
    }
}
