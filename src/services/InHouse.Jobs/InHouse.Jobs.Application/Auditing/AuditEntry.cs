namespace InHouse.Jobs.Application.Auditing;

public sealed record AuditEntry(
    Guid TenantId,
    string? ActorId,
    string Action,
    string Resource,
    string? ResourceId,
    bool Success,
    DateTime OccurredOnUtc,
    string CorrelationId,
    string RequestId,
    string? Ip,
    string? UserAgent,
    string? MetadataJson);