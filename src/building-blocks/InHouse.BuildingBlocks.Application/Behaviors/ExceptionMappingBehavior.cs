using InHouse.BuildingBlocks.Application.Abstractions;
using InHouse.BuildingBlocks.Primitives;
using MediatR;
using Microsoft.Extensions.Logging;

namespace InHouse.BuildingBlocks.Application.Behaviors;

/// <summary>
/// Exception'ı Result tabanlı response'a map’ler.
/// Result olmayan response’larda exception propagate eder.
/// </summary>
public sealed class ExceptionMappingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IExceptionToErrorMapper _mapper;
    private readonly ILogger<ExceptionMappingBehavior<TRequest, TResponse>> _logger;

    public ExceptionMappingBehavior(
        IExceptionToErrorMapper mapper,
        ILogger<ExceptionMappingBehavior<TRequest, TResponse>> logger)
    {
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        try
        {
            return await next();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception in {RequestType}", typeof(TRequest).Name);

            var error = _mapper.Map(ex);

            if (ResultResponseFactory.TryCreateFailure<TResponse>([error], out var failure))
                return failure!;

            throw;
        }
    }
}
