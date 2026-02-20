using InHouse.BuildingBlocks.Abstractions.Integration.Bus;
using InHouse.BuildingBlocks.Abstractions.Integration.Replay;
using InHouse.BuildingBlocks.Api.Integration.Bus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InHouse.BuildingBlocks.Messaging.Kafka;

public static class DependencyInjection
{
    public static IServiceCollection AddKafkaMessaging(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<KafkaOptions>(config.GetSection("Kafka"));

        // Serializer (transport-neutral)
        services.AddSingleton<IIntegrationEventEnvelopeSerializer, JsonIntegrationEventEnvelopeSerializer>();

        // Publisher
        services.AddSingleton<IMessageBusPublisher, KafkaMessageBusPublisher>();

        // DLQ publisher
        services.AddSingleton<KafkaDlqPublisher>();

        // Subscriber host
        services.AddHostedService<KafkaIntegrationEventSubscriberHostedService>();

        services.AddSingleton<IEventReplayService, KafkaEventReplayService>();

        return services;
    }
}