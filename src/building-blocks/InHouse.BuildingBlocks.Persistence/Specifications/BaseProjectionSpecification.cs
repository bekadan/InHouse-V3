using InHouse.BuildingBlocks.Persistence.Specifications;
using System.Linq.Expressions;

namespace InHouse.BuildingBlocks.Persistence.Specification;

public abstract class BaseProjectionSpecification<T, TResult>
    : BaseSpecification<T>, IProjectionSpecification<T, TResult>
{
    protected BaseProjectionSpecification(
        Expression<Func<T, TResult>> selector)
    {
        Selector = selector;
    }

    protected BaseProjectionSpecification(
        Expression<Func<T, bool>> criteria,
        Expression<Func<T, TResult>> selector)
        : base(criteria)
    {
        Selector = selector;
    }

    public Expression<Func<T, TResult>> Selector { get; }
}
