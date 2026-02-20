using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InHouse.Jobs.Domain;
using InHouse.Jobs.Domain.Entities;

namespace InHouse.Jobs.Persistence.Configurations;

public sealed class JobConfiguration : IEntityTypeConfiguration<Job>
{
    public void Configure(EntityTypeBuilder<Job> builder)
    {
        builder.ToTable("jobs");

        builder.HasKey(j => j.Id);

        builder.Property(j => j.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(j => j.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(j => j.CompanyId)
            .IsRequired();

        builder.Property(j => j.CreatedOnUtc)
            .IsRequired();

        builder.HasIndex(j => j.CompanyId);
        builder.HasIndex(j => j.CreatedOnUtc);

        builder.Property(j => j.RowVersion)
            .IsRowVersion();

        builder.HasIndex(j => j.CompanyId);
        builder.HasIndex(j => j.CreatedOnUtc);
        builder.HasIndex(j => new { j.CompanyId, j.IsDeleted });
    }
}