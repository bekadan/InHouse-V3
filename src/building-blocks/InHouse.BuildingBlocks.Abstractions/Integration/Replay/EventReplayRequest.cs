namespace InHouse.BuildingBlocks.Abstractions.Integration.Replay;

public sealed record EventReplayRequest(
    string TenantId,
    string? EventType,
    int? EventVersion,
    DateTime? OccurredFromUtc,
    DateTime? OccurredToUtc,
    bool ForceReprocess,
    string RequestedBy,
    string Reason);