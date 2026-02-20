using InHouse.BuildingBlocks.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace InHouse.Jobs.Persistence.Outbox;

public sealed class OutboxDispatcherHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxDispatcherHostedService> _logger;
    private readonly IClock _clock;

    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan LeaseTimeout = TimeSpan.FromMinutes(5);
    private const int BatchSize = 50;

    private const int MaxRetryCount = 10;

    private readonly string _processorId =
        $"{Environment.MachineName}:{Environment.ProcessId}:{Guid.CreateVersion7():N}";

    public OutboxDispatcherHostedService(
       IServiceScopeFactory scopeFactory,
       ILogger<OutboxDispatcherHostedService> logger,
       IClock clock)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _clock = clock;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Outbox dispatcher started. ProcessorId={ProcessorId}", _processorId);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var dispatched = await DispatchOnce(stoppingToken);
                if (!dispatched)
                    await Task.Delay(PollInterval, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Outbox dispatcher loop error.");
                await Task.Delay(PollInterval, stoppingToken);
            }
        }
    }

    private async Task<bool> DispatchOnce(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<JobsDbContext>();
        var bus = scope.ServiceProvider.GetRequiredService<IEventBus>();

        // 1) Claim batch (kısa transaction + SKIP LOCKED)
        var now = _clock.UtcNow;
        var staleBefore = now - LeaseTimeout;

        List<OutboxMessage> claimed;
        await using (var tx = await db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct))
        {
            // UPDATE ... WHERE id IN (SELECT ... FOR UPDATE SKIP LOCKED) RETURNING *
            const string sql = @"
UPDATE outbox_messages
SET
  ""ProcessingStartedOnUtc"" = @now,
  ""ProcessorId"" = @processorId,
  ""AttemptCount"" = ""AttemptCount"" + 1,
  ""Error"" = NULL
WHERE ""Id"" IN (
  SELECT ""Id""
  FROM outbox_messages
  WHERE ""ProcessedOnUtc"" IS NULL
    AND ""DeadLetteredOnUtc"" IS NULL
    AND (""ProcessingStartedOnUtc"" IS NULL OR ""ProcessingStartedOnUtc"" < @staleBefore)
  ORDER BY ""CreatedOnUtc""
  FOR UPDATE SKIP LOCKED
  LIMIT @take
)
RETURNING *;";

            var pNow = new NpgsqlParameter("now", now);
            var pProc = new NpgsqlParameter("processorId", _processorId);
            var pStale = new NpgsqlParameter("staleBefore", staleBefore);
            var pTake = new NpgsqlParameter("take", BatchSize);

            claimed = await db.OutboxMessages
                .FromSqlRaw(sql, pNow, pProc, pStale, pTake)
                .AsTracking()
                .ToListAsync(ct);

            await tx.CommitAsync(ct);
        }

        if (claimed.Count == 0)
            return false;

        // 2) Publish outside transaction
        foreach (var msg in claimed)
        {
            var t0 = System.Diagnostics.Stopwatch.GetTimestamp();

            try
            {
                await bus.PublishAsync(msg.Type, msg.PayloadJson, ct);
                msg.MarkProcessed(_clock.UtcNow);

                OutboxMetrics.PublishedCount.Add(1);
            }
            catch (Exception ex)
            {
                if (msg.AttemptCount >= MaxRetryCount)
                {
                    msg.MarkDeadLettered(_clock.UtcNow, ex.Message);
                    OutboxMetrics.DeadLetteredCount.Add(1);
                    _logger.LogCritical("Outbox message {MessageId} dead-lettered after {Attempts}", msg.Id, msg.AttemptCount);
                }
                else
                {
                    msg.MarkFailed(ex.Message);
                    msg.ReleaseClaimForRetry();
                    OutboxMetrics.FailedCount.Add(1);
                }
            }
            finally
            {
                var elapsedMs = (System.Diagnostics.Stopwatch.GetTimestamp() - t0) * 1000.0 / System.Diagnostics.Stopwatch.Frequency;
                OutboxMetrics.PublishDurationMs.Record(elapsedMs);
            }
        }

        // 3) Persist processed / error updates
        await db.SaveChangesAsync(ct);
        return true;
    }
}