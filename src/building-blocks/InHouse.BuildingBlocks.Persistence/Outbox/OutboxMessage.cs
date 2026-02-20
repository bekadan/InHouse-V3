namespace InHouse.BuildingBlocks.Persistence.Outbox;

public sealed class OutboxMessage
{
    public Guid Id { get; private set; }
    public Guid? TenantId { get; set; }
    public string EventType { get; set; } = default!;
    public int EventVersion { get; set; }
    public DateTime OccurredOnUtc { get; private set; }
    public string Type { get; private set; } = default!;
    public string PayloadJson { get; private set; } = default!;
    public string? HeadersJson { get; private set; }

    public DateTime CreatedOnUtc { get; private set; }
    public DateTime? ProcessedOnUtc { get; private set; }
    public string? Error { get; private set; }
    public int AttemptCount { get; private set; }

    private OutboxMessage() { } // EF

    public OutboxMessage(
        Guid id,
        DateTime occurredOnUtc,
        string type,
        string payloadJson,
        string? headersJson,
        DateTime createdOnUtc)
    {
        if (id == Guid.Empty) throw new ArgumentException("Outbox message id cannot be empty.", nameof(id));
        if (string.IsNullOrWhiteSpace(type)) throw new ArgumentException("Type is required.", nameof(type));
        if (string.IsNullOrWhiteSpace(payloadJson)) throw new ArgumentException("PayloadJson is required.", nameof(payloadJson));

        Id = id;
        OccurredOnUtc = occurredOnUtc;
        Type = type;
        PayloadJson = payloadJson;
        HeadersJson = headersJson;
        CreatedOnUtc = createdOnUtc;
        AttemptCount = 0;
        EventType = type;
    }

    public void MarkProcessed(DateTime processedOnUtc)
    {
        ProcessedOnUtc = processedOnUtc;
        Error = null;
    }

    public void MarkFailed(DateTime processedOnUtc, string error)
    {
        ProcessedOnUtc = null;
        AttemptCount++;
        Error = string.IsNullOrWhiteSpace(error) ? "Unknown error" : error;
    }

    public static OutboxMessage Create(
        Guid? tenantId,
        string eventType,
        int eventVersion,
        DateTime occurredOnUtc,
        string payloadJson)
    {
        if (string.IsNullOrWhiteSpace(eventType)) throw new ArgumentException("eventType required", nameof(eventType));
        if (string.IsNullOrWhiteSpace(payloadJson)) throw new ArgumentException("payloadJson required", nameof(payloadJson));

        var message = new OutboxMessage();

        message.SetCore(
            tenantId,
            eventType,
            eventVersion,
            occurredOnUtc,
            payloadJson);

        return message;
    }

    // ✅ private setters varsa bu yardımcı method ile tek yerden set edelim
    private void SetCore(
        Guid? tenantId,
        string eventType,
        int eventVersion,
        DateTime occurredOnUtc,
        string payloadJson)
    {
        TenantId = tenantId;
        EventType = eventType;
        EventVersion = eventVersion;
        OccurredOnUtc = occurredOnUtc;
        PayloadJson = payloadJson;
    }
}
