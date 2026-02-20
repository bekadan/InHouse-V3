namespace InHouse.BuildingBlocks.Abstractions;

public interface IEmbeddingService
{
    Task<float[]> GenerateAsync(string text, CancellationToken ct);
}
