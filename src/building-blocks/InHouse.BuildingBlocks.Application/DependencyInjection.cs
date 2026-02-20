using InHouse.BuildingBlocks.Application.Abstractions;
using InHouse.BuildingBlocks.Application.Behaviors;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace InHouse.BuildingBlocks.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddInHouseBuildingBlocksApplication(this IServiceCollection services)
    {
        services.AddSingleton<IExceptionToErrorMapper, DefaultExceptionToErrorMapper>();

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ExceptionMappingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(QueryCachingBehavior<,>));

        return services;
    }
}
