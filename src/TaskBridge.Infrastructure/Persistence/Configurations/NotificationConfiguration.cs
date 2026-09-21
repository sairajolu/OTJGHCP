using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskBridge.Domain.Entities;

namespace TaskBridge.Infrastructure.Persistence.Configurations;

/// <summary>Configures tenant-scoped notification persistence.</summary>
public sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.HasKey(notification => notification.Id);
        builder.Property(notification => notification.EventType).HasConversion<int>().IsRequired();
        builder.Property(notification => notification.Title).HasMaxLength(200).IsRequired();
        builder.Property(notification => notification.Message).HasMaxLength(2000).IsRequired();
        builder.Property(notification => notification.CreatedAtUtc).IsRequired();
        builder.Property(notification => notification.IsRead).IsRequired();

        builder.HasIndex(notification => new
        {
            notification.OrganisationId,
            notification.RecipientUserId,
            notification.CreatedAtUtc
        });
        builder.HasIndex(notification => new
        {
            notification.OrganisationId,
            notification.RecipientUserId,
            notification.IsRead
        });
        builder.HasIndex(notification => new
        {
            notification.OrganisationId,
            notification.EventId,
            notification.RecipientUserId
        }).IsUnique();
    }
}
