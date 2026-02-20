using InHouse.BuildingBlocks.Abstractions;
using System.Security.Claims;

namespace InHouse.Jobs.Api.Security;

public sealed class HttpCurrentActor : ICurrentActor
{
    private readonly IHttpContextAccessor _http;

    public HttpCurrentActor(IHttpContextAccessor http) => _http = http;

    public string? ActorId
        => _http.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier)
           ?? _http.HttpContext?.User?.FindFirstValue("sub");
}