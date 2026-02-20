using InHouse.Jobs.Application.Abstractions;
using InHouse.Jobs.Application.Integration.Commands;
using InHouse.Jobs.Domain.Entities;
using MediatR;

namespace InHouse.Jobs.Application.Integration.Handlers;

public sealed class ApplyJobPostedIntegrationEventCommandHandler
    : IRequestHandler<ApplyJobPostedIntegrationEventCommand>
{
    private readonly IJobRepository _repository;

    public ApplyJobPostedIntegrationEventCommandHandler(IJobRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(
        ApplyJobPostedIntegrationEventCommand request,
        CancellationToken ct)
    {
        var existing = await _repository.GetByIdAsync(request.JobId, ct);
        if (existing is not null)
            return;

        var job = new Job(
            request.JobId,
            request.CompanyId,
            request.Title,
            request.ActorId);

        await _repository.AddAsync(job, ct);
    }
}