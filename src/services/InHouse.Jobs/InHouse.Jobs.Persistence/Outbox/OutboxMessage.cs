using System;

namespace InHouse.Jobs.Persistence.Outbox;

public sealed class OutboxMessage
{
    public Guid Id { get; private set; }
    public DateTime OccurredOnUtc { get; private set; }

    public string Type { get; private set; } = default!;
    public string PayloadJson { get; private set; } = default!;

    public DateTime CreatedOnUtc { get; private set; }
    public DateTime? ProcessedOnUtc { get; private set; }
    public string? Error { get; private set; }

    public DateTime? ProcessingStartedOnUtc { get; private set; }
    public string? ProcessorId { get; private set; }
    public int AttemptCount { get; private set; }

    public DateTime? DeadLetteredOnUtc { get; private set; }

    public bool IsDeadLettered => DeadLetteredOnUtc.HasValue;

    private OutboxMessage() { } // EF

    public OutboxMessage(Guid id, DateTime occurredOnUtc, string type, string payloadJson)
    {
        Id = id;
        OccurredOnUtc = occurredOnUtc;
        Type = type;
        PayloadJson = payloadJson;
        CreatedOnUtc = DateTime.UtcNow;
        AttemptCount = 0;
    }

    public void MarkClaimed(DateTime utcNow, string processorId)
    {
        ProcessingStartedOnUtc = utcNow;
        ProcessorId = processorId;
        AttemptCount++;
        Error = null;
    }

    public void MarkProcessed(DateTime utcNow)
    {
        ProcessedOnUtc = utcNow;
        ProcessingStartedOnUtc = null;
        ProcessorId = null;
        Error = null;
    }

    public void MarkFailed(string error)
    {
        Error = error;
        // retry için claim’i bırakıyoruz (lease timeout ile tekrar alınacak)
        // istersen burada ProcessingStartedOnUtc = null yapıp hemen retry da yaptırabiliriz.
    }

    public void ReleaseClaimForRetry()
    {
        ProcessingStartedOnUtc = null;
        ProcessorId = null;
    }

    public void MarkDeadLettered(DateTime utcNow, string reason)
    {
        DeadLetteredOnUtc = utcNow;
        Error = reason;
        ProcessingStartedOnUtc = null;
        ProcessorId = null;
    }
}