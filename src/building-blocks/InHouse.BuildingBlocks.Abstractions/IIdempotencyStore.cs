namespace InHouse.BuildingBlocks.Abstractions;

public interface IIdempotencyStore
{
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
    Task StoreAsync(string key, CancellationToken cancellationToken = default);
}
