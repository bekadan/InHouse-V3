using InHouse.BuildingBlocks.Abstractions;
using InHouse.BuildingBlocks.Application.Abstractions;
using InHouse.BuildingBlocks.Application.Requests;
using InHouse.BuildingBlocks.Primitives;
using MediatR;

namespace InHouse.BuildingBlocks.Application.Behaviors;

public sealed class AuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IAuthorizationService _authorization;
    private readonly ICurrentUser _currentUser;
    private readonly ITenantContext _tenantContext;

    public AuthorizationBehavior(
        IAuthorizationService authorization,
        ICurrentUser currentUser,
        ITenantContext tenantContext)
    {
        _authorization = authorization;
        _currentUser = currentUser;
        _tenantContext = tenantContext;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not IAuthorizableRequest authz)
            return await next();

        if (!_currentUser.IsAuthenticated)
        {
            if (ResultResponseFactory.TryCreateFailure<TResponse>(
                    [DomainErrorCatalog.Auth.Unauthorized], out var unauth))
                return unauth!;
        }

        var ok = await _authorization.AuthorizeAsync(
            _currentUser.UserId,
            _tenantContext.TenantId,
            _currentUser.Roles,
            authz.Policy,
            cancellationToken);

        if (!ok)
        {
            if (ResultResponseFactory.TryCreateFailure<TResponse>(
                    [DomainErrorCatalog.Auth.Forbidden], out var forbidden))
                return forbidden!;
        }

        return await next();
    }
}
