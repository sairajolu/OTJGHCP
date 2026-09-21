using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskBridge.Domain.Entities;

namespace TaskBridge.Infrastructure.Persistence.Configurations;

/// <summary>Configures immutable audit entry persistence.</summary>
public sealed class AuditEntryConfiguration : IEntityTypeConfiguration<AuditEntry>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<AuditEntry> builder)
    {
        builder.HasKey(entry => entry.Id);
        builder.Property(entry => entry.EventType).HasConversion<int>().IsRequired();
        builder.Property(entry => entry.EntityType).HasMaxLength(100).IsRequired();
        builder.Property(entry => entry.CorrelationId).HasMaxLength(100).IsRequired();
        builder.Property(entry => entry.ActorIpAddress).HasMaxLength(45);
        builder.Property(entry => entry.PreviousState).HasColumnType("TEXT").HasMaxLength(32000);
        builder.Property(entry => entry.NewState).HasColumnType("TEXT").HasMaxLength(32000);
        builder.Property(entry => entry.TimestampUtc).IsRequired();
        builder.Property(entry => entry.CreatedAtUtc).IsRequired();

        builder.HasIndex(entry => new { entry.OrganisationId, entry.EventId }).IsUnique();
        builder.HasIndex(entry => new { entry.OrganisationId, entry.ProjectId, entry.TimestampUtc });
        builder.HasIndex(entry => new { entry.OrganisationId, entry.ProjectId, entry.EventType });
    }
}
