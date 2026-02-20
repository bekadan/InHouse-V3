namespace InHouse.BuildingBlocks.Abstractions.Integration.Bus;

public interface IMessageBusSubscriber
{
    Task StartAsync(CancellationToken ct);
    Task StopAsync(CancellationToken ct);
}