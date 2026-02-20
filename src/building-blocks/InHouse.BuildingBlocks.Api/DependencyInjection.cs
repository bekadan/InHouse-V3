using InHouse.BuildingBlocks.Abstractions;
using InHouse.BuildingBlocks.Abstractions.Integration.Inbox;
using InHouse.BuildingBlocks.Abstractions.Integration.Versioning;
using InHouse.BuildingBlocks.Abstractions.Messaging;
using InHouse.BuildingBlocks.Api.Integration.Inbox;
using InHouse.BuildingBlocks.Api.Integration.Publishing;
using InHouse.BuildingBlocks.Api.Integration.Versioning;
using InHouse.BuildingBlocks.Api.Middleware;
using InHouse.BuildingBlocks.Api.Observability;
using InHouse.BuildingBlocks.Api.Security;
using InHouse.BuildingBlocks.Api.Tenancy;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace InHouse.BuildingBlocks.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddInHouseBuildingBlocksApi(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();

        // Abstractions impl
        services.AddSingleton<IClock, SystemClock>();
        services.AddScoped<ITenantContext, HttpTenantContext>();
        services.AddScoped<ICurrentUser, HttpCurrentUser>();
        services.AddScoped<IEventContextAccessor, HttpEventContextAccessor>();
        services.AddScoped<IIntegrationEventPublisher, MessageBusIntegrationEventPublisher>();
        services.AddScoped<InboxBypassScope>();
        services.AddScoped<IInboxBypassScope>(sp => sp.GetRequiredService<InboxBypassScope>());
        return services;
    }

    public static IApplicationBuilder UseInHouseBuildingBlocksApi(this IApplicationBuilder app)
    {
        // Exception en dışta olsun
        app.UseMiddleware<GlobalExceptionMiddleware>();

        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseMiddleware<TenantResolutionMiddleware>();

        return app;
    }

    public static IServiceCollection AddIntegrationVersioning(this IServiceCollection services)
    {
        services.AddSingleton<IEventUpcasterRegistry, DefaultEventUpcasterRegistry>();
        services.AddSingleton<IEventUpcastingPipeline, EventUpcastingPipeline>();
        return services;
    }
}
