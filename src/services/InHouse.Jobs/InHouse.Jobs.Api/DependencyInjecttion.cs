using InHouse.BuildingBlocks.Abstractions.Integration.Consumers;
using InHouse.BuildingBlocks.Abstractions.Integration.Inbox;
using InHouse.BuildingBlocks.Api.Integration.Consumers;
using InHouse.BuildingBlocks.Persistence.Inbox;
using InHouse.Jobs.Application.Integration.Consumers;
using InHouse.Jobs.Persistence;

namespace InHouse.Jobs.Api
{
    public static class IntegrationConsumersDi
    {
        public static IServiceCollection AddIntegrationConsumerInfrastructure(this IServiceCollection services)
        {
            // Runner
            services.AddScoped<IntegrationEventConsumerRunner>();

            // Inbox store bound to service write context
            services.AddScoped<IInboxStore, EfCoreInboxStore<JobsDbContext>>();

            return services;
        }
    }

    public static class JobsHandlersDi
    {
        public static IServiceCollection AddJobsIntegrationHandlers(this IServiceCollection services)
        {
            services.AddScoped<IIntegrationEventHandler, JobPostedV1Handler>();
            return services;
        }
    }
}
