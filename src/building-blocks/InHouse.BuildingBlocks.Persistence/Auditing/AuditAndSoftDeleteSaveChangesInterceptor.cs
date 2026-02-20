using InHouse.BuildingBlocks.Abstractions;
using InHouse.BuildingBlocks.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace InHouse.BuildingBlocks.Persistence.Auditing;

public sealed class AuditAndSoftDeleteSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly IClock _clock;
    private readonly ICurrentActor _actor;

    public AuditAndSoftDeleteSaveChangesInterceptor(IClock clock, ICurrentActor actor)
    {
        _clock = clock;
        _actor = actor;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
            Apply(eventData.Context);

        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
            Apply(eventData.Context);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void Apply(DbContext db)
    {
        var now = _clock.UtcNow;
        var actorId = _actor.ActorId;

        foreach (var entry in db.ChangeTracker.Entries().Where(e => e.Entity is not null))
        {
            // Audit
            if (entry.Entity is IAuditableEntity auditable)
            {
                if (entry.State == EntityState.Added)
                    auditable.SetCreated(now, actorId);

                if (entry.State == EntityState.Modified)
                    auditable.SetModified(now, actorId);
            }

            // Soft delete: physical delete yerine state modify + flag
            if (entry.State == EntityState.Deleted && entry.Entity is ISoftDeletableEntity soft)
            {
                soft.SoftDelete(now);
                entry.State = EntityState.Modified;
            }
        }
    }
}