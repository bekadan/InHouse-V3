using InHouse.BuildingBlocks.Abstractions;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace InHouse.BuildingBlocks.Api.Security;

public sealed class HttpCurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _http;

    public HttpCurrentUser(IHttpContextAccessor http) => _http = http;

    public bool IsAuthenticated
        => _http.HttpContext?.User?.Identity?.IsAuthenticated == true;

    public string? UserId
        => FindFirstValue(ClaimTypes.NameIdentifier) ?? FindFirstValue("sub");

    public string? Email
        => FindFirstValue(ClaimTypes.Email) ?? FindFirstValue("email");

    public IReadOnlyCollection<string> Roles
        => _http.HttpContext?.User?.FindAll(ClaimTypes.Role).Select(x => x.Value).Distinct().ToArray()
           ?? Array.Empty<string>();

    private string? FindFirstValue(string claimType)
        => _http.HttpContext?.User?.FindFirst(claimType)?.Value;
}
