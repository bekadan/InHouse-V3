using InHouse.BuildingBlocks.Application.Abstractions;
using InHouse.BuildingBlocks.Application.Requests;
using MediatR;
using Microsoft.Extensions.Logging;

namespace InHouse.BuildingBlocks.Application.Behaviors;

public sealed class QueryCachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ICacheStore _cache;
    private readonly ILogger<QueryCachingBehavior<TRequest, TResponse>> _logger;

    public QueryCachingBehavior(ICacheStore cache, ILogger<QueryCachingBehavior<TRequest, TResponse>> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not ICacheableQuery cacheable)
            return await next();

        if (request is ICacheBypassableQuery bypass && bypass.BypassCache)
            return await next();

        var cached = await _cache.GetAsync<TResponse>(cacheable.CacheKey, cancellationToken);
        if (cached is not null)
        {
            _logger.LogDebug("Cache hit: {Key}", cacheable.CacheKey);
            return cached;
        }

        _logger.LogDebug("Cache miss: {Key}", cacheable.CacheKey);

        var response = await next();

        // Null response’ları cache’lemeyelim (isteğe bağlı).
        if (response is not null)
            await _cache.SetAsync(cacheable.CacheKey, response, cacheable.TimeToLive, cancellationToken);

        return response;
    }
}
