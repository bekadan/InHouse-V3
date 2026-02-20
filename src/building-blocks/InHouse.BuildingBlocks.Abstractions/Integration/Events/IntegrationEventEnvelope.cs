using InHouse.BuildingBlocks.Abstractions.Messaging;
using System.Text.Json;

namespace InHouse.BuildingBlocks.Abstractions.Integration.Events;

public sealed record IntegrationEventEnvelope(
    string MessageId,
    Guid? TenantId,
    string EventType,
    int EventVersion,
    DateTime OccurredOnUtc,
    string PayloadJson,
    IReadOnlyDictionary<string, string>? Headers)
{
    public static IntegrationEventEnvelope Create(
        Guid? tenantId,
        IIntegrationEvent @event,
        DateTime occurredOnUtc,
        IReadOnlyDictionary<string, string>? headers = null)
    {

        var eventType = @event.GetType().Name; // Eğer repoda EventType string’i varsa onu kullan
        if (@event is IHasIntegrationEventType typed)
            eventType = typed.EventType;

        var version = (@event as IVersionedIntegrationEvent)?.Version ?? 1;

        var payloadJson = JsonSerializer.Serialize(@event, @event.GetType());

        return new IntegrationEventEnvelope(
            MessageId: Guid.NewGuid().ToString("N"),
            TenantId: tenantId,
            EventType: eventType,
            EventVersion: version,
            OccurredOnUtc: occurredOnUtc,
            PayloadJson: payloadJson,
            Headers: headers);
    }
}

/// <summary>
/// Eğer repo’da IIntegrationEvent üzerinde type yoksa, bu küçük interface ile taşırız.
/// Varsa bunu eklemeyebilirsin.
/// </summary>
public interface IHasIntegrationEventType
{
    string EventType { get; }
}