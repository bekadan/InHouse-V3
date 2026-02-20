using InHouse.BuildingBlocks.Abstractions.Integration.Bus;
using InHouse.BuildingBlocks.Abstractions.Integration.Events;
using InHouse.BuildingBlocks.Abstractions.Messaging;
using InHouse.BuildingBlocks.Abstractions.MultiTenancy; // ITenantProvider burada değilse using'i düzelt
using InHouse.BuildingBlocks.Persistence.Tenancy;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json;

namespace InHouse.BuildingBlocks.Api.Integration.Publishing;

public sealed class MessageBusIntegrationEventPublisher : IIntegrationEventPublisher
{
    private static readonly ActivitySource ActivitySource =
        new("InHouse.Integration.Publishing");

    private readonly IMessageBusPublisher _busPublisher;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<MessageBusIntegrationEventPublisher> _logger;

    public MessageBusIntegrationEventPublisher(
        IMessageBusPublisher busPublisher,
        ITenantProvider tenantProvider,
        ILogger<MessageBusIntegrationEventPublisher> logger)
    {
        _busPublisher = busPublisher ?? throw new ArgumentNullException(nameof(busPublisher));
        _tenantProvider = tenantProvider ?? throw new ArgumentNullException(nameof(tenantProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task PublishAsync(IIntegrationEvent @event, CancellationToken cancellationToken = default)
    {
        if (@event is null)
            throw new ArgumentNullException(nameof(@event));

        // ✅ Repo’da GetTenantId() yok. Burada TenantId'yi property’den alıyoruz.
        // Eğer property adı farklıysa aşağıdaki satırı uyarlayacaksın (notlar en altta).
        var tenantId = _tenantProvider.TenantId;

        var envelope = IntegrationEventEnvelope.Create(
            tenantId: tenantId,
            @event: @event,
            occurredOnUtc: DateTime.UtcNow);

        using var activity = ActivitySource.StartActivity("integration.publish", ActivityKind.Producer);

        activity?.SetTag("messaging.system", "inhouse");
        activity?.SetTag("messaging.message_id", envelope.MessageId);
        activity?.SetTag("tenant.id", envelope.TenantId);
        activity?.SetTag("messaging.event_type", envelope.EventType);
        activity?.SetTag("messaging.event_version", envelope.EventVersion);

        try
        {
            await _busPublisher.PublishAsync(envelope, cancellationToken).ConfigureAwait(false);

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
            throw;
        }
        catch (Exception ex)
        {
            activity?.SetTag("integration.publish.success", false);

            _logger.LogError(
                ex,
                "Failed to publish integration event. EventType={EventType}, TenantId={TenantId}",
                envelope.EventType,
                envelope.TenantId);

            throw;
        }
    }
}