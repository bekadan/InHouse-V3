using InHouse.BuildingBlocks.Persistence.Db;
using InHouse.BuildingBlocks.Persistence.Inbox;
using Microsoft.EntityFrameworkCore;

namespace InHouse.BuildingBlocks.Persistence;

public abstract class InHouseWriteDbContext : DbContext
{
    protected InHouseWriteDbContext(DbContextOptions options) : base(options) { }

    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // other building blocks: outbox, tenants, soft-delete, etc...
        modelBuilder.AddInbox();
    }
}