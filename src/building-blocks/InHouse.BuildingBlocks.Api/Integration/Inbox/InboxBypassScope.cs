using InHouse.BuildingBlocks.Abstractions.Integration.Inbox;

namespace InHouse.BuildingBlocks.Api.Integration.Inbox;

public sealed class InboxBypassScope : IInboxBypassScope
{
    private bool _enabled;

    public bool IsEnabled => _enabled;

    public IDisposable Enable()
    {
        _enabled = true;
        return new Reset(() => _enabled = false);
    }

    private sealed class Reset : IDisposable
    {
        private readonly Action _reset;
        private bool _disposed;

        public Reset(Action reset) => _reset = reset;

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _reset();
        }
    }
}