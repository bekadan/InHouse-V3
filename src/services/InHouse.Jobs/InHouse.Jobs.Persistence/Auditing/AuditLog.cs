using System;

namespace InHouse.Jobs.Persistence.Auditing;

public sealed class AuditLog
{
    public Guid Id { get; private set; }

    public Guid TenantId { get; private set; }
    public string? ActorId { get; private set; }

    public string Action { get; private set; } = default!;
    public string Resource { get; private set; } = default!;
    public string? ResourceId { get; private set; }

    public bool Success { get; private set; }

    public DateTime OccurredOnUtc { get; private set; }

    public string CorrelationId { get; private set; } = default!;
    public string RequestId { get; private set; } = default!;

    public string? Ip { get; private set; }
    public string? UserAgent { get; private set; }

    public string? MetadataJson { get; private set; }

    private AuditLog() { } // EF

    public AuditLog(
        Guid id,
        Guid tenantId,
        string? actorId,
        string action,
        string resource,
        string? resourceId,
        bool success,
        DateTime occurredOnUtc,
        string correlationId,
        string requestId,
        string? ip,
        string? userAgent,
        string? metadataJson)
    {
        Id = id;
        TenantId = tenantId;
        ActorId = actorId;

        Action = action;
        Resource = resource;
        ResourceId = resourceId;

        Success = success;

        OccurredOnUtc = occurredOnUtc;

        CorrelationId = correlationId;
        RequestId = requestId;

        Ip = ip;
        UserAgent = userAgent;
        MetadataJson = metadataJson;
    }
}