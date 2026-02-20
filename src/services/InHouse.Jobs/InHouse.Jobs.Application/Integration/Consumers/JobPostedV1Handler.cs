using System.Text.Json;
using InHouse.BuildingBlocks.Abstractions.Integration.Consumers;
using InHouse.BuildingBlocks.Abstractions.Integration.Events;
using InHouse.Jobs.Application.Integration.Commands;
using MediatR;

namespace InHouse.Jobs.Application.Integration.Consumers;

public sealed class JobPostedV1Handler : IIntegrationEventHandler
{
    private readonly IMediator _mediator;

    public JobPostedV1Handler(IMediator mediator) => _mediator = mediator;

    public string ConsumerName => "Jobs.JobPostedV1";
    public string EventType => "Jobs.JobPosted";
    public int EventVersion => 1;

    public async Task HandleAsync(IntegrationEventEnvelope envelope, CancellationToken ct)
    {
        var payload = JsonSerializer.Deserialize<JobPostedV1Payload>(envelope.PayloadJson)
                      ?? throw new InvalidOperationException("Invalid payload.");

        // Delegate into your Application layer command/query (CQRS)
        await _mediator.Send(new ApplyJobPostedIntegrationEventCommand(
            payload.JobId,
            payload.CompanyId,
            payload.Title,
            envelope.Headers?.TryGetValue("actor-id", out var actor)
                == true ? actor : "system"
        ), ct);
    }

    private sealed record JobPostedV1Payload(
        Guid JobId,
        string Title,
        Guid CompanyId);
}