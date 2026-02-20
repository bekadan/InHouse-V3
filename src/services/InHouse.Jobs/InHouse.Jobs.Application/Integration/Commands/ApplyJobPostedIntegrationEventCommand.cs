using MediatR;

namespace InHouse.Jobs.Application.Integration.Commands;

/// <summary>
/// Internal application command triggered by integration event.
/// Keeps transport concerns out of domain.
/// </summary>
public sealed record ApplyJobPostedIntegrationEventCommand(
    Guid JobId,
    Guid CompanyId,
    string Title,
    string? ActorId) : IRequest;