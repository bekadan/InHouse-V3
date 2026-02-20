using InHouse.BuildingBlocks.Abstractions.Integration.Versioning;

namespace InHouse.BuildingBlocks.Api.Integration.Versioning;

public sealed class DefaultEventUpcasterRegistry : IEventUpcasterRegistry
{
    private readonly Dictionary<(string EventType, int FromVersion), IEventUpcaster> _map;
    private readonly Dictionary<string, int> _latest;

    public DefaultEventUpcasterRegistry(IEnumerable<IEventUpcaster> upcasters)
    {
        _map = new Dictionary<(string, int), IEventUpcaster>(StringTupleComparer.Ordinal);
        _latest = new Dictionary<string, int>(StringComparer.Ordinal);

        foreach (var u in upcasters)
        {
            var key = (u.EventType, u.FromVersion);
            if (_map.ContainsKey(key))
                throw new InvalidOperationException($"Duplicate upcaster registered for {u.EventType} v{u.FromVersion}.");

            _map[key] = u;

            var current = _latest.TryGetValue(u.EventType, out var v) ? v : 0;
            _latest[u.EventType] = Math.Max(current, u.ToVersion);
        }
    }

    public IEventUpcaster? Find(string eventType, int fromVersion)
        => _map.TryGetValue((eventType, fromVersion), out var u) ? u : null;

    public int GetLatestVersion(string eventType)
        => _latest.TryGetValue(eventType, out var v) ? v : 0;

    private sealed class StringTupleComparer : IEqualityComparer<(string EventType, int FromVersion)>
    {
        public static readonly StringTupleComparer Ordinal = new();

        public bool Equals((string EventType, int FromVersion) x, (string EventType, int FromVersion) y)
            => x.FromVersion == y.FromVersion && StringComparer.Ordinal.Equals(x.EventType, y.EventType);

        public int GetHashCode((string EventType, int FromVersion) obj)
            => HashCode.Combine(StringComparer.Ordinal.GetHashCode(obj.EventType), obj.FromVersion);
    }
}