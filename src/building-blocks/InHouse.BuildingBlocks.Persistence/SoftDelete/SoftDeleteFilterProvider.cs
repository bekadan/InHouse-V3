namespace InHouse.BuildingBlocks.Persistence.SoftDelete;

public sealed class SoftDeleteFilterProvider : ISoftDeleteFilterProvider
{
    private bool _bypass;

    public bool BypassSoftDeleteFilter => _bypass;

    public IDisposable BeginBypassScope()
    {
        _bypass = true;
        return new DisposeAction(() => _bypass = false);
    }

    private sealed class DisposeAction : IDisposable
    {
        private readonly Action _onDispose;
        public DisposeAction(Action onDispose) => _onDispose = onDispose;
        public void Dispose() => _onDispose();
    }
}