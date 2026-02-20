using InHouse.BuildingBlocks.Abstractions;
using InHouse.BuildingBlocks.Abstractions.Messaging;
using InHouse.BuildingBlocks.Application.Abstractions;
using InHouse.BuildingBlocks.Persistence.Auditing;
using InHouse.BuildingBlocks.Persistence.CompiledQueries;
using InHouse.BuildingBlocks.Persistence.Idempotency;
using InHouse.BuildingBlocks.Persistence.Integration.Publishing;
using InHouse.BuildingBlocks.Persistence.Options;
using InHouse.BuildingBlocks.Persistence.Outbox;
using InHouse.BuildingBlocks.Persistence.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace InHouse.BuildingBlocks.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddInHouseBuildingBlocksPersistence<TDbContext>(
        this IServiceCollection services,
        Action<PersistenceOptions>? configure = null)
        where TDbContext : DbContext
    {
        if (configure is not null)
            services.Configure(configure);
        else
            services.AddOptions<PersistenceOptions>();

        services.AddSingleton<IOutboxSerializer>(sp =>
        {
            // Serbest: servis kendi JsonSerializerOptions sağlıyorsa onu kullan
            var options = sp.GetService<JsonSerializerOptions>();
            return new SystemTextJsonOutboxSerializer(options);
        });

        services.AddScoped<SaveChangesInterceptor, OutboxSaveChangesInterceptor>();

        services.AddScoped<IUnitOfWork, EfUnitOfWork<TDbContext>>();

        services.AddScoped<IIdempotencyStore, EfIdempotencyStore<TDbContext>>();

        services.AddSingleton<ICompiledQueryCache, CompiledQueryCache>();

        services.AddScoped<SaveChangesInterceptor, AuditAndSoftDeleteSaveChangesInterceptor>();

        services.AddScoped<SoftDelete.ISoftDeleteFilterProvider, SoftDelete.SoftDeleteFilterProvider>();

        services.AddScoped<IIntegrationEventPublisher, EfCoreIntegrationEventPublisher>();

        return services;
    }
}
