namespace InHouse.BuildingBlocks.Messaging;

public sealed class EventEnvelope
{
    public Guid MessageId { get; init; }
    public string EventType { get; init; } = default!;
    public string Payload { get; init; } = default!;
    public string? Headers { get; init; }
    public DateTime OccurredOnUtc { get; init; }
    public DateTime PublishedOnUtc { get; init; }
}
