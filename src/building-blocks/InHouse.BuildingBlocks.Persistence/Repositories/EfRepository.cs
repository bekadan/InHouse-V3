using InHouse.BuildingBlocks.Persistence.Specification;
using InHouse.BuildingBlocks.Persistence.Specifications;
using Microsoft.EntityFrameworkCore;

namespace InHouse.BuildingBlocks.Persistence.Repositories;

public sealed class EfRepository<T> : IRepository<T>
where T : class
{
    private readonly DbContext _db;

    public EfRepository(DbContext db) => _db = db;

    public Task<T?> GetByIdAsync(object id, CancellationToken cancellationToken = default)
        => _db.Set<T>().FindAsync([id], cancellationToken).AsTask();

    public async Task<T?> FirstOrDefaultAsync(ISpecification<T> spec, CancellationToken cancellationToken = default)
    {
        var query = ApplySpecification(spec);
        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<T>> ListAsync(ISpecification<T> spec, CancellationToken cancellationToken = default)
    {
        var query = ApplySpecification(spec);
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(ISpecification<T> spec, CancellationToken cancellationToken = default)
    {
        var query = ApplySpecification(spec);
        return await query.CountAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(ISpecification<T> spec, CancellationToken cancellationToken = default)
    {
        var query = ApplySpecification(spec);
        return await query.AnyAsync(cancellationToken);
    }

    public async Task<List<TResult>> ListAsync<TResult>(
        IProjectionSpecification<T, TResult> spec,
        CancellationToken cancellationToken = default)
    {
        var query = SpecificationEvaluator<T>
            .GetQuery(_db.Set<T>().AsQueryable(), spec);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<TResult?> FirstOrDefaultAsync<TResult>(
        IProjectionSpecification<T, TResult> spec,
        CancellationToken cancellationToken = default)
    {
        var query = SpecificationEvaluator<T>
            .GetQuery(_db.Set<T>().AsQueryable(), spec);

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public void Add(T entity) => _db.Set<T>().Add(entity);

    public void Update(T entity) => _db.Set<T>().Update(entity);

    public void Remove(T entity) => _db.Set<T>().Remove(entity);

    private IQueryable<T> ApplySpecification(ISpecification<T> spec)
        => SpecificationEvaluator<T>.GetQuery(_db.Set<T>().AsQueryable(), spec);
}
