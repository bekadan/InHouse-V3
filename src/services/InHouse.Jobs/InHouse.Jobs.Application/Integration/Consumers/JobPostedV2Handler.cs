using System.Text.Json;
using InHouse.BuildingBlocks.Abstractions.Integration.Consumers;
using InHouse.BuildingBlocks.Abstractions.Integration.Events;
using InHouse.Jobs.Application.Integration.Commands;
using MediatR;

namespace InHouse.Jobs.Application.Integration.Consumers;

public sealed class JobPostedV2Handler : IIntegrationEventHandler
{
    private readonly IMediator _mediator;

    public JobPostedV2Handler(IMediator mediator) => _mediator = mediator;

    public string ConsumerName => "Jobs.JobPostedV2";
    public string EventType => "Jobs.JobPosted";
    public int EventVersion => 2;

    public async Task HandleAsync(IntegrationEventEnvelope envelope, CancellationToken ct)
    {
        var payload = JsonSerializer.Deserialize<JobPostedV2Payload>(envelope.PayloadJson)
                      ?? throw new InvalidOperationException("Invalid payload.");

        var actor =
            envelope.Headers?.TryGetValue("actor-id", out var a) == true
                ? a
                : "system";

        await _mediator.Send(new ApplyJobPostedIntegrationEventCommand(
            JobId: payload.JobId,
            CompanyId: payload.CompanyId,
            Title: payload.Title,
            ActorId: actor
        ), ct);
    }

    private sealed record JobPostedV2Payload(
        Guid JobId,
        Guid CompanyId,
        string Title,
        string EmploymentType);
}