namespace InHouse.BuildingBlocks.Abstractions.Integration.Events;

/// <summary>
/// Transport-neutral integration event envelope.
/// MessageId MUST be stable across retries (bus delivery id or producer outbox message id).
/// TenantId is required for multi-tenant isolation.
/// </summary>
public sealed record IntegrationEventEnvelope(
    string MessageId,
    string TenantId,
    string EventType,
    int EventVersion,
    DateTime OccurredOnUtc,
    string PayloadJson,
    IReadOnlyDictionary<string, string>? Headers = null);