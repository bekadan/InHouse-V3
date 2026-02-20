using FluentValidation;
using InHouse.BuildingBlocks.Primitives;
using MediatR;

namespace InHouse.BuildingBlocks.Application.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        => _validators = validators;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);

        var failures = (await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken))))
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count == 0)
            return await next();

        // Result tabanlı response dönüyorsak failure üretelim; değilse exception.
        var errors = failures
            .Select(f => Error.Validation(
                code: "validation.failed",
                message: $"{f.PropertyName}: {f.ErrorMessage}"))
            .ToList();

        if (ResultResponseFactory.TryCreateFailure<TResponse>(errors, out var failure))
            return failure!;

        throw new ValidationException(failures);
    }
}
