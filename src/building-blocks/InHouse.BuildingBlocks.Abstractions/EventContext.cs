namespace InHouse.BuildingBlocks.Abstractions;

public sealed record EventContext(
    string CorrelationId,
    string? TenantId,
    string? ActorId,
    string Source,
    string RequestId,
    DateTime OccurredOnUtc
)
{
    public static EventContext Create(
        string correlationId,
        string? tenantId,
        string? actorId,
        string source,
        string requestId,
        DateTime utcNow)
        => new(
            correlationId,
            tenantId,
            actorId,
            source,
            requestId,
            utcNow);
}
