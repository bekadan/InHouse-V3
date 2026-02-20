namespace InHouse.BuildingBlocks.Abstractions.Integration.Inbox;

/// <summary>
/// When enabled (admin-controlled), consumer idempotency checks can be bypassed for reprocessing/replay.
/// Must be scoped.
/// </summary>
public interface IInboxBypassScope
{
    bool IsEnabled { get; }
}