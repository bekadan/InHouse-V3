using InHouse.BuildingBlocks.Persistence.Specifications;
using System.Linq.Expressions;

public abstract class DynamicOrderSpecification<T> : BaseSpecification<T>
{
    protected void ApplyDynamicOrder(
        string? sortField,
        bool descending,
        Dictionary<string, Expression<Func<T, object>>> allowed)
    {
        if (string.IsNullOrWhiteSpace(sortField))
            return;

        if (!allowed.TryGetValue(sortField.ToLowerInvariant(), out var expression))
            return;

        if (descending)
            ApplyOrderByDescending(expression);
        else
            ApplyOrderBy(expression);
    }
}