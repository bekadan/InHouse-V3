using InHouse.BuildingBlocks.Abstractions;
using InHouse.BuildingBlocks.Api.Http;
using Microsoft.AspNetCore.Http;

namespace InHouse.BuildingBlocks.Api.Observability;

public sealed class HttpEventContextAccessor : IEventContextAccessor
{
    private readonly IHttpContextAccessor _http;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;

    public HttpEventContextAccessor(
        IHttpContextAccessor http,
        ITenantContext tenantContext,
        ICurrentUser currentUser,
        IClock clock)
    {
        _http = http;
        _tenantContext = tenantContext;
        _currentUser = currentUser;
        _clock = clock;
    }

    public EventContext Current
    {
        get
        {
            var ctx = _http.HttpContext;

            // Worker/background senaryosunda HttpContext olmayabilir:
            var correlationId = ctx?.Request.Headers[InHouseHeaders.CorrelationId].ToString();
            if (string.IsNullOrWhiteSpace(correlationId))
                correlationId = Guid.CreateVersion7().ToString("N");

            var requestId = ctx?.Request.Headers[InHouseHeaders.RequestId].ToString();
            if (string.IsNullOrWhiteSpace(requestId))
                requestId = ctx?.TraceIdentifier ?? Guid.CreateVersion7().ToString("N");

            var source = ctx?.Request.Headers[InHouseHeaders.Source].ToString();
            if (string.IsNullOrWhiteSpace(source))
                source = "API";

            return EventContext.Create(
                correlationId: correlationId!,
                tenantId: _tenantContext.TenantId,
                actorId: _currentUser.UserId,
                source: source!,
                requestId: requestId!,
                utcNow: _clock.UtcNow);
        }
    }
}
