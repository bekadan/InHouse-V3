using InHouse.BuildingBlocks.Abstractions.Integration.Consumers;
using InHouse.Jobs.Application.Auditing;
using InHouse.Jobs.Application.Integration.Consumers;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddJobsApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
        });

        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(AuditCommandBehavior<,>));

        return services;
    }
}