using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace InHouse.BuildingBlocks.Messaging;

public static class DependencyInjection
{
    public static IServiceCollection AddInHouseMessaging<TDbContext>(
        this IServiceCollection services,
        Action<OutboxPublisherOptions>? configure = null)
        where TDbContext : DbContext
    {
        if (configure != null)
            services.Configure(configure);

        services.AddSingleton<IEventBus, InMemoryEventBus>();

        services.AddHostedService<OutboxPublisherWorker<TDbContext>>();

        return services;
    }
}
