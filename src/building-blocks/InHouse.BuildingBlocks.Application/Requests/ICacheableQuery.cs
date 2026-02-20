namespace InHouse.BuildingBlocks.Application.Requests;

public interface ICacheableQuery
{
    string CacheKey { get; }
    TimeSpan TimeToLive { get; }
}

public interface ICacheBypassableQuery
{
    bool BypassCache { get; }
}
