using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace InHouse.Jobs.Persistence.Outbox;

public sealed class OutboxGaugeProvider : IDisposable
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxGaugeProvider> _logger;

    private readonly Meter _meter = new(OutboxMetrics.MeterName);
    private readonly ObservableGauge<long> _pendingGauge;
    private readonly ObservableGauge<long> _deadGauge;
    private readonly ObservableGauge<long> _lagSecondsGauge;

    private long _pending;
    private long _dead;
    private long _lagSeconds;

    private readonly CancellationTokenSource _cts = new();
    private readonly Task _loop;

    public OutboxGaugeProvider(IServiceScopeFactory scopeFactory, ILogger<OutboxGaugeProvider> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;

        _pendingGauge = _meter.CreateObservableGauge<long>("outbox_pending", () => _pending);
        _deadGauge = _meter.CreateObservableGauge<long>("outbox_deadlettered", () => _dead);
        _lagSecondsGauge = _meter.CreateObservableGauge<long>("outbox_lag_seconds", () => _lagSeconds);

        _loop = Task.Run(UpdateLoop);
    }

    private async Task UpdateLoop()
    {
        while (!_cts.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<JobsDbContext>();

                var pendingQ = db.OutboxMessages.Where(x => x.ProcessedOnUtc == null && x.DeadLetteredOnUtc == null);

                _pending = await pendingQ.CountAsync(_cts.Token);
                _dead = await db.OutboxMessages.CountAsync(x => x.DeadLetteredOnUtc != null, _cts.Token);

                var oldest = await pendingQ
                    .OrderBy(x => x.CreatedOnUtc)
                    .Select(x => (DateTime?)x.CreatedOnUtc)
                    .FirstOrDefaultAsync(_cts.Token);

                _lagSeconds = oldest is null ? 0 : (long)(DateTime.UtcNow - oldest.Value).TotalSeconds;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to update outbox gauges.");
            }

            await Task.Delay(TimeSpan.FromSeconds(10), _cts.Token);
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
        try { _loop.Wait(TimeSpan.FromSeconds(2)); } catch { /* ignore */ }
        _cts.Dispose();
        _meter.Dispose();
    }
}