using InHouse.BuildingBlocks.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace InHouse.BuildingBlocks.Persistence.UnitOfWork;

public sealed class EfUnitOfWork<TDbContext> : IUnitOfWork
    where TDbContext : DbContext
{
    private readonly TDbContext _db;
    private IDbContextTransaction? _tx;

    public EfUnitOfWork(TDbContext db) => _db = db;

    public async Task BeginAsync(CancellationToken cancellationToken = default)
    {
        if (_tx is not null) return;

        _tx = await _db.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        if (_tx is null) return;

        await _db.SaveChangesAsync(cancellationToken);
        await _tx.CommitAsync(cancellationToken);
        await _tx.DisposeAsync();
        _tx = null;
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (_tx is null) return;

        await _tx.RollbackAsync(cancellationToken);
        await _tx.DisposeAsync();
        _tx = null;
    }
}
