using InHouse.BuildingBlocks.Abstractions;
using InHouse.BuildingBlocks.Abstractions.Messaging;
using InHouse.Jobs.Application.Abstractions;
using InHouse.Jobs.Application.Auditing;
using InHouse.Jobs.Application.Queries;
using InHouse.Jobs.Persistence.Auditing;
using InHouse.Jobs.Persistence.Outbox;
using InHouse.Jobs.Persistence.ReadServices;
using InHouse.Jobs.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace InHouse.Jobs.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddJobsPersistence(
        this IServiceCollection services,
        string writeConnectionString,
        string? readConnectionString = null)
    {
        // WRITE: migrations + outbox + transactions
        services.AddDbContext<JobsDbContext>(options =>
            options.UseNpgsql(writeConnectionString));

        // READ: no-tracking + compiled queries (istersen read-replica cs)
        var readCs = string.IsNullOrWhiteSpace(readConnectionString) ? writeConnectionString : readConnectionString;

        // Pool önerilir (read için çok iyi)
        services.AddDbContextPool<JobsReadDbContext>(options =>
            options.UseNpgsql(readCs));

        services.AddScoped<IJobReadService, JobReadService>();

        services.AddDbContext<JobsDbContext>((sp, options) =>
        {
            options.UseNpgsql(writeConnectionString);
            options.AddInterceptors(sp.GetServices<SaveChangesInterceptor>());
        });

        services.AddScoped<IAuditLogger, EfAuditLogger>();

        services.AddScoped<IIntegrationEventPublisher, EfOutboxIntegrationEventPublisher>();

        services.AddSingleton<IEventBus, InMemoryEventBus>();

        services.AddHostedService<OutboxDispatcherHostedService>();

        services.AddScoped<OutboxHealthCheck>();

        services.AddScoped<IJobRepository, JobRepository>();

        return services;
    }
}