namespace InHouse.BuildingBlocks.Abstractions.Integration.Replay;

public sealed record EventReplayResult(
    Guid ReplayId,
    int Scanned,
    int Republished,
    int SkippedByFilter);