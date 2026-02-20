using InHouse.BuildingBlocks.Persistence.Tenancy;

namespace InHouse.Jobs.Api.Tenancy;

public sealed class HttpTenantProvider : ITenantProvider
{
    private readonly IHttpContextAccessor _http;

    private bool _bypass;

    public HttpTenantProvider(IHttpContextAccessor http)
        => _http = http;

    public Guid? TenantId
    {
        get
        {
            if (_bypass)
                return null;

            var ctx = _http.HttpContext;
            if (ctx is null) return null;

            if (ctx.Request.Headers.TryGetValue("X-Tenant-Id", out var v) &&
                Guid.TryParse(v.ToString(), out var id))
                return id;

            return null;
        }
    }

    public bool BypassTenantFilter => _bypass;

    public IDisposable BeginBypassScope()
    {
        _bypass = true;
        return new DisposeAction(() => _bypass = false);
    }

    private sealed class DisposeAction : IDisposable
    {
        private readonly Action _onDispose;
        public DisposeAction(Action onDispose) => _onDispose = onDispose;
        public void Dispose() => _onDispose();
    }
}