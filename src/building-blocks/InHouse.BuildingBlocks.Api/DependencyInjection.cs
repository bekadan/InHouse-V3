using InHouse.BuildingBlocks.Abstractions;
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
}
