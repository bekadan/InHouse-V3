using InHouse.BuildingBlocks.Persistence.Specification;
using Microsoft.EntityFrameworkCore;

namespace InHouse.BuildingBlocks.Persistence.Specifications;

public static class SpecificationEvaluator<T> where T : class
{
    public static IQueryable<T> GetQuery(
        IQueryable<T> inputQuery,
        ISpecification<T> spec)
    {
        var query = inputQuery;

        if (spec.Criteria != null)
            query = query.Where(spec.Criteria);

        if (spec.OrderBy != null)
            query = query.OrderBy(spec.OrderBy);
        else if (spec.OrderByDescending != null)
            query = query.OrderByDescending(spec.OrderByDescending);

        if (spec.Skip.HasValue)
            query = query.Skip(spec.Skip.Value);

        if (spec.Take.HasValue)
            query = query.Take(spec.Take.Value);

        query = spec.Includes.Aggregate(query,
            (current, include) => current.Include(include));

        query = spec.IncludeStrings.Aggregate(query,
            (current, include) => current.Include(include));

        if (spec.AsSplitQuery)
            query = query.AsSplitQuery();

        if (spec.AsNoTracking)
            query = query.AsNoTracking();

        return query;
    }

    public static IQueryable<TResult> GetQuery<T, TResult>(
        IQueryable<T> inputQuery,
        IProjectionSpecification<T, TResult> spec)
        where T : class
    {
        var query = SpecificationEvaluator<T>.GetQuery(inputQuery, (ISpecification<T>)spec);

        return query.Select(spec.Selector);
    }
}
