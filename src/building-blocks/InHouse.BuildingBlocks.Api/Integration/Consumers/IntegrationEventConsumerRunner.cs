using System.Diagnostics;
using InHouse.BuildingBlocks.Abstractions.Integration.Consumers;
using InHouse.BuildingBlocks.Abstractions.Integration.Events;
using InHouse.BuildingBlocks.Abstractions.Integration.Inbox;
using InHouse.BuildingBlocks.Abstractions.MultiTenancy;
using Microsoft.Extensions.Logging;

namespace InHouse.BuildingBlocks.Api.Integration.Consumers;

public sealed class IntegrationEventConsumerRunner
{
    private static readonly ActivitySource ActivitySource = new("InHouse.Integration.Consumers");

    private readonly IInboxStore _inbox;
    private readonly ITenantScopeFactory _tenantScopeFactory;
    private readonly ILogger<IntegrationEventConsumerRunner> _logger;

    public IntegrationEventConsumerRunner(
        IInboxStore inbox,
        ITenantScopeFactory tenantScopeFactory,
        ILogger<IntegrationEventConsumerRunner> logger)
    {
        _inbox = inbox;
        _tenantScopeFactory = tenantScopeFactory;
        _logger = logger;
    }

    public async Task ConsumeAsync(
        IIntegrationEventHandler handler,
        IntegrationEventEnvelope envelope,
        CancellationToken ct)
    {
        var nowUtc = DateTime.UtcNow;

        using var activity = ActivitySource.StartActivity(
            $"{handler.ConsumerName} {envelope.EventType} v{envelope.EventVersion}",
            ActivityKind.Consumer);

        activity?.SetTag("messaging.message_id", envelope.MessageId);
        activity?.SetTag("messaging.event_type", envelope.EventType);
        activity?.SetTag("messaging.event_version", envelope.EventVersion);
        activity?.SetTag("tenant.id", envelope.TenantId);
        activity?.SetTag("consumer.name", handler.ConsumerName);

        // lease duration should be tuned; keep it small and rely on retries
        var lease = await _inbox.TryAcquireLeaseAsync(
            tenantId: envelope.TenantId,
            consumerName: handler.ConsumerName,
            messageId: envelope.MessageId,
            nowUtc: nowUtc,
            leaseDuration: TimeSpan.FromSeconds(60),
            ct: ct);

        if (lease is null)
        {
            _logger.LogDebug(
                "Skipping message {MessageId} for consumer {Consumer} (already processed or leased).",
                envelope.MessageId, handler.ConsumerName);
            activity?.SetTag("inbox.skipped", true);
            return;
        }

        try
        {
            using (_tenantScopeFactory.BeginScope(envelope.TenantId))
            {
                // version gate (we’ll extend into full versioning strategy next)
                if (handler.EventType != envelope.EventType || handler.EventVersion != envelope.EventVersion)
                {
                    throw new InvalidOperationException(
                        $"Handler mismatch. Handler={handler.EventType} v{handler.EventVersion}, Envelope={envelope.EventType} v{envelope.EventVersion}");
                }

                await handler.HandleAsync(envelope, ct);
            }

            await _inbox.MarkProcessedAsync(
                envelope.TenantId,
                handler.ConsumerName,
                envelope.MessageId,
                lease.LeaseId,
                DateTime.UtcNow,
                ct);

            activity?.SetTag("inbox.processed", true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed consuming message {MessageId} for consumer {Consumer}.",
                envelope.MessageId, handler.ConsumerName);

            await _inbox.RecordFailureAsync(
                envelope.TenantId,
                handler.ConsumerName,
                envelope.MessageId,
                lease.LeaseId,
                DateTime.UtcNow,
                ex.ToString(),
                ct);

            activity?.SetTag("inbox.failed", true);
            throw; // let the transport retry
        }
    }
}