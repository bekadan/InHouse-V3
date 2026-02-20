using System;

namespace InHouse.BuildingBlocks.Abstractions.Integration.Inbox;

public sealed record InboxLease(
    Guid LeaseId,
    DateTime LeaseExpiresOnUtc);

public interface IInboxStore
{
    /// <summary>
    /// Attempts to acquire a processing lease for (tenant, consumer, messageId).
    /// Returns null if the message was already processed OR another instance currently holds a valid lease.
    /// </summary>
    Task<InboxLease?> TryAcquireLeaseAsync(
        string tenantId,
        string consumerName,
        string messageId,
        DateTime nowUtc,
        TimeSpan leaseDuration,
        CancellationToken ct);

    Task MarkProcessedAsync(
        string tenantId,
        string consumerName,
        string messageId,
        Guid leaseId,
        DateTime processedOnUtc,
        CancellationToken ct);

    Task RecordFailureAsync(
        string tenantId,
        string consumerName,
        string messageId,
        Guid leaseId,
        DateTime failedOnUtc,
        string error,
        CancellationToken ct);
}