using InHouse.BuildingBlocks.Abstractions.Integration.Bus;
using InHouse.BuildingBlocks.Abstractions.Integration.Events;
using InHouse.BuildingBlocks.Abstractions.Messaging;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace InHouse.BuildingBlocks.Api.Integration.Publishing;

/// <summary>
/// Default integration event publisher that forwards envelopes
/// to the configured message bus implementation (Kafka, etc.).
/// 
/// This class is transport-agnostic and relies on IMessageBusPublisher
/// to perform the actual delivery.
/// </summary>
public sealed class MessageBusIntegrationEventPublisher : IIntegrationEventPublisher
{
    private static readonly ActivitySource ActivitySource =
        new("InHouse.Integration.Publishing");

    private readonly IMessageBusPublisher _busPublisher;
    private readonly ILogger<MessageBusIntegrationEventPublisher> _logger;

    public MessageBusIntegrationEventPublisher(
        IMessageBusPublisher busPublisher,
        ILogger<MessageBusIntegrationEventPublisher> logger)
    {
        _busPublisher = busPublisher ?? throw new ArgumentNullException(nameof(busPublisher));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Publishes a fully constructed integration event envelope
    /// to the configured message bus.
    /// </summary>
    public async Task PublishAsync(
        IntegrationEventEnvelope envelope,
        CancellationToken cancellationToken = default)
    {
        if (envelope is null)
            throw new ArgumentNullException(nameof(envelope));

        using var activity = ActivitySource.StartActivity(
            "integration.publish",
            ActivityKind.Producer);

        activity?.SetTag("messaging.system", "inhouse");
        activity?.SetTag("messaging.destination_kind", "topic");
        activity?.SetTag("messaging.message_id", envelope.MessageId);
        activity?.SetTag("tenant.id", envelope.TenantId);
        activity?.SetTag("messaging.event_type", envelope.EventType);
        activity?.SetTag("messaging.event_version", envelope.EventVersion);

        try
        {
            await _busPublisher.PublishAsync(envelope, cancellationToken)
                               .ConfigureAwait(false);

            activity?.SetTag("integration.publish.success", true);

            _logger.LogInformation(
                "Integration event published. MessageId={MessageId}, EventType={EventType}, TenantId={TenantId}",
                envelope.MessageId,
                envelope.EventType,
                envelope.TenantId);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            activity?.SetTag("integration.publish.cancelled", true);

            _logger.LogWarning(
                "Integration event publishing cancelled. MessageId={MessageId}",
                envelope.MessageId);

            throw;
        }
        catch (Exception ex)
        {
            activity?.SetTag("integration.publish.success", false);

            _logger.LogError(
                ex,
                "Failed to publish integration event. MessageId={MessageId}, EventType={EventType}, TenantId={TenantId}",
                envelope.MessageId,
                envelope.EventType,
                envelope.TenantId);

            throw;
        }
    }
}