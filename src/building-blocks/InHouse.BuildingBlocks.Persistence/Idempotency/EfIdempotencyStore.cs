using InHouse.BuildingBlocks.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace InHouse.BuildingBlocks.Persistence.Idempotency;

public sealed class EfIdempotencyStore<TDbContext> : IIdempotencyStore
    where TDbContext : DbContext
{
    private readonly TDbContext _db;

    public EfIdempotencyStore(TDbContext db) => _db = db;

    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        key = Normalize(key);
        return _db.Set<IdempotencyRecord>().AnyAsync(x => x.Key == key, cancellationToken);
    }

    public async Task StoreAsync(string key, CancellationToken cancellationToken = default)
    {
        key = Normalize(key);

        _db.Set<IdempotencyRecord>().Add(new IdempotencyRecord(key, DateTime.UtcNow));
        await _db.SaveChangesAsync(cancellationToken);
    }

    private static string Normalize(string key)
        => string.IsNullOrWhiteSpace(key) ? throw new ArgumentException("Idempotency key required.", nameof(key)) : key.Trim();
}
