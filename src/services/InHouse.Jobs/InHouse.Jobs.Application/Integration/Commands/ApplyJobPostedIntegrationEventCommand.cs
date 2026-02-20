using MediatR;

namespace InHouse.Jobs.Application.Integration.Commands;

public sealed record ApplyJobPostedIntegrationEventCommand(
    Guid JobId,
    Guid CompanyId,
    string Title,
    string? ActorId
) : IRequest;