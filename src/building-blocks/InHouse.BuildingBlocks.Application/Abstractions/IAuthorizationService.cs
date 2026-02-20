namespace InHouse.BuildingBlocks.Application.Abstractions;

public interface IAuthorizationService
{
    Task<bool> AuthorizeAsync(
        string? userId,
        string? tenantId,
        IReadOnlyCollection<string> roles,
        string policy,
        CancellationToken cancellationToken = default);
}
