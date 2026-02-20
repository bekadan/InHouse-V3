using System;

namespace InHouse.BuildingBlocks.Persistence.Inbox;

public sealed class InboxMessage
{
    public long Id { get; private set; }

    public required string TenantId { get; init; }
    public required string ConsumerName { get; init; }
    public required string MessageId { get; init; }

    public required DateTime ReceivedOnUtc { get; init; }

    public Guid? LeaseId { get; private set; }
    public DateTime? LeaseExpiresOnUtc { get; private set; }

    public DateTime? ProcessedOnUtc { get; private set; }

    public int AttemptCount { get; private set; }
    public DateTime? LastAttemptOnUtc { get; private set; }
    public string? LastError { get; private set; }

    public void AcquireLease(Guid leaseId, DateTime leaseExpiresOnUtc, DateTime nowUtc)
    {
        LeaseId = leaseId;
        LeaseExpiresOnUtc = leaseExpiresOnUtc;
        AttemptCount++;
        LastAttemptOnUtc = nowUtc;
        LastError = null;
    }

    public void MarkProcessed(DateTime processedOnUtc)
    {
        ProcessedOnUtc = processedOnUtc;
        // keep lease info for audit/debug or null it; we’ll null it to reduce confusion
        LeaseId = null;
        LeaseExpiresOnUtc = null;
    }

    public void RecordFailure(DateTime failedOnUtc, string error)
    {
        LastAttemptOnUtc = failedOnUtc;
        LastError = error;
        // release lease so another instance can retry after backoff policy externally
        LeaseId = null;
        LeaseExpiresOnUtc = null;
    }
}