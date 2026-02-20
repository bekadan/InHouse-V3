using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace InHouse.Jobs.Persistence.Outbox;

public sealed class OutboxHealthCheck : IHealthCheck
{
    private readonly JobsDbContext _db;
    private static readonly TimeSpan MaxLag = TimeSpan.FromMinutes(10);
    private static readonly int MaxBacklog = 5_000;

    public OutboxHealthCheck(JobsDbContext db) => _db = db;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var pendingQuery = _db.OutboxMessages
            .Where(x => x.ProcessedOnUtc == null && x.DeadLetteredOnUtc == null);

        var pending = await pendingQuery.CountAsync(cancellationToken);

        var dead = await _db.OutboxMessages
            .Where(x => x.DeadLetteredOnUtc != null)
            .CountAsync(cancellationToken);

        var oldestCreated = await pendingQuery
            .OrderBy(x => x.CreatedOnUtc)
            .Select(x => (DateTime?)x.CreatedOnUtc)
            .FirstOrDefaultAsync(cancellationToken);

        var lag = oldestCreated is null ? TimeSpan.Zero : (DateTime.UtcNow - oldestCreated.Value);

        var data = new Dictionary<string, object?>
        {
            ["pending"] = pending,
            ["deadLettered"] = dead,
            ["lagSeconds"] = (int)lag.TotalSeconds
        };

        // Degraded: biraz backlog var ama sistem çalışıyor
        if (pending > MaxBacklog || lag > MaxLag)
            return HealthCheckResult.Degraded("Outbox backlog/lag is high.", data: data);

        return HealthCheckResult.Healthy("Outbox is healthy.", data: data);
    }
}