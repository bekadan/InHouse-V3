using InHouse.BuildingBlocks.Abstractions.Integration.Inbox;
using Microsoft.EntityFrameworkCore;

namespace InHouse.BuildingBlocks.Persistence.Inbox;

public sealed class EfCoreInboxStore<TWriteDbContext> : IInboxStore
    where TWriteDbContext : DbContext
{
    private readonly TWriteDbContext _db;
    private readonly IInboxBypassScope _bypass;

    public EfCoreInboxStore(TWriteDbContext db, IInboxBypassScope bypass) {
        _db = db;
        _bypass = bypass;
    }

    public async Task<InboxLease?> TryAcquireLeaseAsync(
        string tenantId,
        string consumerName,
        string messageId,
        DateTime nowUtc,
        TimeSpan leaseDuration,
        CancellationToken ct)
    {
        // 1) Ensure row exists (idempotent insert). If it already exists, ignore.
        // Using raw SQL is acceptable here for correctness + perf; still no domain leakage.
        // Table/column names match our configuration.
        var receivedOn = nowUtc;

        // Insert-once pattern
        await _db.Database.ExecuteSqlInterpolatedAsync($@"
INSERT INTO ""InboxMessages"" (""TenantId"", ""ConsumerName"", ""MessageId"", ""ReceivedOnUtc"", ""AttemptCount"")
VALUES ({tenantId}, {consumerName}, {messageId}, {receivedOn}, 0)
ON CONFLICT (""TenantId"", ""ConsumerName"", ""MessageId"") DO NOTHING;
", ct);

        // 2) If already processed => no lease
        var existing = await _db.Set<InboxMessage>()
            .AsTracking()
            .SingleAsync(x => x.TenantId == tenantId
                          && x.ConsumerName == consumerName
                          && x.MessageId == messageId, ct);

        if (existing.ProcessedOnUtc is not null && !_bypass.IsEnabled)
            return null;

        // 3) Acquire lease only if none exists OR expired
        var leaseId = Guid.NewGuid();
        var leaseExpires = nowUtc.Add(leaseDuration);

        var canTakeLease =
            existing.LeaseId is null
            || existing.LeaseExpiresOnUtc is null
            || existing.LeaseExpiresOnUtc <= nowUtc;

        if (!canTakeLease)
            return null;

        // optimistic: set and save
        existing.AcquireLease(leaseId, leaseExpires, nowUtc);

        try
        {
            await _db.SaveChangesAsync(ct);
            return new InboxLease(leaseId, leaseExpires);
        }
        catch (DbUpdateConcurrencyException)
        {
            // another instance raced us
            return null;
        }
        catch (DbUpdateException)
        {
            // another instance might have updated lease/processed in between
            return null;
        }
    }

    public async Task MarkProcessedAsync(
        string tenantId,
        string consumerName,
        string messageId,
        Guid leaseId,
        DateTime processedOnUtc,
        CancellationToken ct)
    {
        var msg = await _db.Set<InboxMessage>()
            .AsTracking()
            .SingleAsync(x => x.TenantId == tenantId
                          && x.ConsumerName == consumerName
                          && x.MessageId == messageId, ct);

        // If lease doesn't match, someone else took over; ignore (at-least-once safety)
        if (msg.LeaseId != leaseId && msg.ProcessedOnUtc is not null)
            return;

        if (msg.LeaseId != leaseId && msg.ProcessedOnUtc is null)
            throw new InvalidOperationException("Inbox lease lost before processing completed.");

        msg.MarkProcessed(processedOnUtc);
        await _db.SaveChangesAsync(ct);
    }

    public async Task RecordFailureAsync(
        string tenantId,
        string consumerName,
        string messageId,
        Guid leaseId,
        DateTime failedOnUtc,
        string error,
        CancellationToken ct)
    {
        var msg = await _db.Set<InboxMessage>()
            .AsTracking()
            .SingleAsync(x => x.TenantId == tenantId
                          && x.ConsumerName == consumerName
                          && x.MessageId == messageId, ct);

        // if we lost lease, don't overwrite the other processor’s state
        if (msg.LeaseId != leaseId)
            return;

        msg.RecordFailure(failedOnUtc, error);
        await _db.SaveChangesAsync(ct);
    }
}