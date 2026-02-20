using InHouse.BuildingBlocks.Abstractions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace InHouse.BuildingBlocks.Application.Behaviors;

public sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
    private readonly IEventContextAccessor _eventContextAccessor;

    public LoggingBehavior(
        ILogger<LoggingBehavior<TRequest, TResponse>> logger,
        IEventContextAccessor eventContextAccessor)
    {
        _logger = logger;
        _eventContextAccessor = eventContextAccessor;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var ctx = _eventContextAccessor.Current;

        using var scope = _logger.BeginScope(new Dictionary<string, object?>
        {
            ["correlationId"] = ctx.CorrelationId,
            ["tenantId"] = ctx.TenantId,
            ["actorId"] = ctx.ActorId,
            ["source"] = ctx.Source,
            ["requestId"] = ctx.RequestId,
            ["requestType"] = typeof(TRequest).Name
        });

        _logger.LogInformation("Handling {RequestType}", typeof(TRequest).Name);

        var started = System.Diagnostics.Stopwatch.StartNew();
        var response = await next();
        started.Stop();

        _logger.LogInformation("Handled {RequestType} in {ElapsedMs}ms",
            typeof(TRequest).Name, started.ElapsedMilliseconds);

        return response;
    }
}