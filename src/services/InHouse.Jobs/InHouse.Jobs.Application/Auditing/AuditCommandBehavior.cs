using InHouse.BuildingBlocks.Abstractions;
using InHouse.BuildingBlocks.Abstractions.Messaging;
using InHouse.BuildingBlocks.Persistence.Tenancy;
using MediatR;
using System.Threading;

namespace InHouse.Jobs.Application.Auditing;

public sealed class AuditCommandBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IAuditLogger _auditLogger;
    private readonly ITenantProvider _tenant;
    private readonly ICurrentActor _actor;
    private readonly IClock _clock;
    private readonly IEventContextAccessor _ctx;
    private readonly IIntegrationEventPublisher _publisher;

    public AuditCommandBehavior(
        IAuditLogger auditLogger,
        ITenantProvider tenant,
        ICurrentActor actor,
        IClock clock,
        IEventContextAccessor ctx, 
        IIntegrationEventPublisher publisher)
    {
        _auditLogger = auditLogger;
        _tenant = tenant;
        _actor = actor;
        _clock = clock;
        _ctx = ctx;
        _publisher = publisher;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not IAuditableCommand auditable)
            return await next();

        var ec = _ctx.Current;
        var started = _clock.UtcNow;

        try
        {
            var response = await next();

            if (Guid.TryParse(ec.TenantId, out var tenantId))
            {
                await _publisher.PublishAsync(
                    new AuditLoggedIntegrationEvent(
                        TenantId: tenantId,
                        ActorId: ec.ActorId,
                        Action: auditable.Action,
                        Resource: auditable.Resource,
                        ResourceId: auditable.ResourceId,
                        Success: true,
                        CorrelationId: ec.CorrelationId,
                        RequestId: ec.RequestId,
                        Source: ec.Source,
                        OccurredOnUtc: started,
                        MetadataJson: null),
                    cancellationToken);
            }

            return response;
        }
        catch (Exception)
        {
            if (Guid.TryParse(ec.TenantId, out var tenantId))
            {
                await _publisher.PublishAsync(
                    new AuditLoggedIntegrationEvent(
                        TenantId: tenantId,
                        ActorId: ec.ActorId,
                        Action: auditable.Action,
                        Resource: auditable.Resource,
                        ResourceId: auditable.ResourceId,
                        Success: false,
                        CorrelationId: ec.CorrelationId,
                        RequestId: ec.RequestId,
                        Source: ec.Source,
                        OccurredOnUtc: started,
                        MetadataJson: null),
                    cancellationToken);
            }

            throw;
        }
    }
}