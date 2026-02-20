using InHouse.BuildingBlocks.Abstractions.Messaging;

namespace InHouse.Jobs.Application.Auditing;

public sealed record AuditLoggedIntegrationEvent(
    Guid TenantId,
    string? ActorId,
    string Action,
    string Resource,
    string? ResourceId,
    bool Success,
    string CorrelationId,
    string RequestId,
    string Source,
    DateTime OccurredOnUtc,
    string? MetadataJson
) : IIntegrationEvent
{
    public string EventType => "audit.logged.v1";
}