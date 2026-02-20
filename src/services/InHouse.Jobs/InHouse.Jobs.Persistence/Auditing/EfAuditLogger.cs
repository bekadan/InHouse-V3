using InHouse.Jobs.Application.Auditing;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace InHouse.Jobs.Persistence.Auditing;

public sealed class EfAuditLogger : IAuditLogger
{
    private readonly JobsDbContext _db;

    public EfAuditLogger(JobsDbContext db) => _db = db;

    public async Task LogAsync(AuditEntry entry, CancellationToken cancellationToken = default)
    {
        var log = new AuditLog(
            id: Guid.CreateVersion7(),
            tenantId: entry.TenantId,
            actorId: entry.ActorId,
            action: entry.Action,
            resource: entry.Resource,
            resourceId: entry.ResourceId,
            success: entry.Success,
            occurredOnUtc: entry.OccurredOnUtc,
            correlationId: entry.CorrelationId,
            requestId: entry.RequestId,
            ip: entry.Ip,
            userAgent: entry.UserAgent,
            metadataJson: entry.MetadataJson);

        _db.AuditLogs.Add(log);

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException)
        {
            // Unique index çakışması = aynı audit zaten yazılmış (idempotent davran)
        }
    }
}