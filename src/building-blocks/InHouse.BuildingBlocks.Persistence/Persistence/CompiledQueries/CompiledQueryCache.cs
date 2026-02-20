using System.Collections.Concurrent;

namespace InHouse.BuildingBlocks.Persistence.CompiledQueries;

public interface ICompiledQueryCache
{
    TDelegate GetOrAdd<TDelegate>(object key, Func<TDelegate> factory)
        where TDelegate : Delegate;
}

public sealed class CompiledQueryCache : ICompiledQueryCache
{
    private readonly ConcurrentDictionary<object, Delegate> _cache = new();

    public TDelegate GetOrAdd<TDelegate>(object key, Func<TDelegate> factory)
        where TDelegate : Delegate
    {
        var del = _cache.GetOrAdd(key, _ => factory());
        return (TDelegate)del;
    }
}