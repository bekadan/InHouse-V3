using InHouse.BuildingBlocks.Persistence.Inbox;
using Microsoft.EntityFrameworkCore;

namespace InHouse.BuildingBlocks.Persistence.Db;

public static class ModelBuilderExtensionsInbox
{
    public static ModelBuilder AddInbox(this ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new InboxMessageEntityTypeConfiguration());
        return modelBuilder;
    }
}