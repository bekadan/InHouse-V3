namespace InHouse.BuildingBlocks.Persistence.SoftDelete;

public interface ISoftDeleteFilterProvider
{
    bool BypassSoftDeleteFilter { get; }
    IDisposable BeginBypassScope();
}