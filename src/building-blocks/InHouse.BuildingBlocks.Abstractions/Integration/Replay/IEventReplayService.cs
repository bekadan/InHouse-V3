namespace InHouse.BuildingBlocks.Abstractions.Integration.Replay;

public interface IEventReplayService
{
    Task<EventReplayResult> ReplayFromDlqAsync(EventReplayRequest request, CancellationToken ct);
}