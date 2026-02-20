using InHouse.BuildingBlocks.Persistence.Specification;
using InHouse.BuildingBlocks.Persistence.Specifications;

namespace InHouse.BuildingBlocks.Persistence.Repositories;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(object id, CancellationToken cancellationToken = default);
    Task<T?> FirstOrDefaultAsync(ISpecification<T> spec, CancellationToken cancellationToken = default);
    Task<List<T>> ListAsync(ISpecification<T> spec, CancellationToken cancellationToken = default);
    Task<int> CountAsync(ISpecification<T> spec, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(ISpecification<T> spec, CancellationToken cancellationToken = default);
    Task<List<TResult>> ListAsync<TResult>(
    IProjectionSpecification<T, TResult> spec,
    CancellationToken cancellationToken = default);
    Task<TResult?> FirstOrDefaultAsync<TResult>(
        IProjectionSpecification<T, TResult> spec,
        CancellationToken cancellationToken = default);

    void Add(T entity);
    void Update(T entity);
    void Remove(T entity);
}
