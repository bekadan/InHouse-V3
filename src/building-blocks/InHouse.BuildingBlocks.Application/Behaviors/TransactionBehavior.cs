using InHouse.BuildingBlocks.Application.Abstractions;
using InHouse.BuildingBlocks.Application.Requests;
using MediatR;
using Microsoft.Extensions.Logging;

namespace InHouse.BuildingBlocks.Application.Behaviors;

public sealed class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IUnitOfWork _uow;
    private readonly ILogger<TransactionBehavior<TRequest, TResponse>> _logger;

    public TransactionBehavior(IUnitOfWork uow, ILogger<TransactionBehavior<TRequest, TResponse>> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Query’lerde transaction açmayalım.
        // Command marker’ını net tutmak için ICommand<> interface’lerini request’lere uygulatacağız.
        var isCommand = request is ICommand<TResponse> || request is ICommand;

        if (!isCommand)
            return await next();

        await _uow.BeginAsync(cancellationToken);

        try
        {
            var response = await next();
            await _uow.CommitAsync(cancellationToken);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Transaction failed for {RequestType}", typeof(TRequest).Name);
            await _uow.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
