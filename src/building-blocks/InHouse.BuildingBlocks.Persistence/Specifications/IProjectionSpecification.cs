using InHouse.BuildingBlocks.Persistence.Specifications;
using System.Linq.Expressions;

namespace InHouse.BuildingBlocks.Persistence.Specification;

public interface IProjectionSpecification<T, TResult> : ISpecification<T>
{
    Expression<Func<T, TResult>> Selector { get; }
}
