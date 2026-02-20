using FluentValidation;
using InHouse.BuildingBlocks.Application.Abstractions;
using InHouse.BuildingBlocks.Domain;
using InHouse.BuildingBlocks.Primitives;

namespace InHouse.BuildingBlocks.Application;

public sealed class DefaultExceptionToErrorMapper : IExceptionToErrorMapper
{
    public Error Map(Exception exception)
    {
        return exception switch
        {
            ValidationException ve => Error.Validation("validation.exception", ve.Message),
            DomainException de => Error.Failure("domain.rule_violation", de.Message),
            UnauthorizedAccessException => DomainErrorCatalog.Auth.Unauthorized,
            _ => DomainErrorCatalog.General.Unexpected
        };
    }
}
