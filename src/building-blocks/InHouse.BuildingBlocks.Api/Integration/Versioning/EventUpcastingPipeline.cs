using System.Diagnostics;
using InHouse.BuildingBlocks.Abstractions.Integration.Events;
using InHouse.BuildingBlocks.Abstractions.Integration.Versioning;
using Microsoft.Extensions.Logging;

namespace InHouse.BuildingBlocks.Api.Integration.Versioning;

public sealed class EventUpcastingPipeline : IEventUpcastingPipeline
{
    private static readonly ActivitySource ActivitySource = new("InHouse.Integration.Versioning");

    private readonly IEventUpcasterRegistry _registry;
    private readonly ILogger<EventUpcastingPipeline> _logger;

    public EventUpcastingPipeline(IEventUpcasterRegistry registry, ILogger<EventUpcastingPipeline> logger)
    {
        _registry = registry;
        _logger = logger;
    }

    public IntegrationEventEnvelope UpcastToLatest(IntegrationEventEnvelope envelope)
    {
        using var activity = ActivitySource.StartActivity("integration.upcast", ActivityKind.Internal);

        activity?.SetTag("messaging.event_type", envelope.EventType);
        activity?.SetTag("messaging.event_version", envelope.EventVersion);
        activity?.SetTag("tenant.id", envelope.TenantId);

        var latest = _registry.GetLatestVersion(envelope.EventType);
        if (latest <= 0 || envelope.EventVersion >= latest)
            return envelope;

        var originalType = envelope.EventType;
        var originalVersion = envelope.EventVersion;

        var current = envelope;
        var guard = 0;

        while (true)
        {
            if (guard++ > 50)
                throw new InvalidOperationException($"Upcast loop guard triggered for {originalType}. Check your upcaster chain.");

            if (current.EventVersion >= latest)
                break;

            var upcaster = _registry.Find(current.EventType, current.EventVersion);
            if (upcaster is null)
            {
                _logger.LogWarning(
                    "No upcaster found for EventType={EventType} FromVersion={FromVersion}. Latest={Latest}.",
                    current.EventType, current.EventVersion, latest);
                break;
            }

            var before = current.EventVersion;
            current = upcaster.Upcast(current);

            _logger.LogDebug(
                "Upcasted {EventType} v{From} -> v{To}. MessageId={MessageId}",
                current.EventType, before, current.EventVersion, current.MessageId);
        }

        if (current.EventType == originalType && current.EventVersion != originalVersion)
        {
            current = current with
            {
                Headers = MergeHeaders(current.Headers,
                    new Dictionary<string, string>
                    {
                        ["x-original-event-version"] = originalVersion.ToString(),
                        ["x-original-event-type"] = originalType
                    })
            };
        }

        activity?.SetTag("integration.upcasted_to_version", current.EventVersion);
        return current;
    }

    private static IReadOnlyDictionary<string, string>? MergeHeaders(
        IReadOnlyDictionary<string, string>? existing,
        IReadOnlyDictionary<string, string> added)
    {
        if (existing is null || existing.Count == 0)
            return new Dictionary<string, string>(added);

        var merged = new Dictionary<string, string>(existing, StringComparer.Ordinal);
        foreach (var kv in added)
            merged[kv.Key] = kv.Value;

        return merged;
    }
}