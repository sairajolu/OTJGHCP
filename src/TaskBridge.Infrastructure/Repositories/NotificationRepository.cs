using Microsoft.EntityFrameworkCore;
using TaskBridge.Application.Abstractions;
using TaskBridge.Application.Notifications;
using TaskBridge.Domain.Entities;
using TaskBridge.Infrastructure.Persistence;

namespace TaskBridge.Infrastructure.Repositories;

/// <summary>Persists notifications with organisation and recipient scoping.</summary>
public sealed class NotificationRepository : INotificationRepository
{
    private readonly TaskBridgeDbContext dbContext;

    /// <summary>Initializes the notification repository.</summary>
    public NotificationRepository(TaskBridgeDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task AddAsync(Notification notification, CancellationToken cancellationToken)
    {
        await dbContext.Notifications.AddAsync(notification, cancellationToken);
    }

    /// <inheritdoc />
    public Task<bool> ExistsByEventIdAndRecipientAsync(
        Guid organisationId,
        Guid eventId,
        Guid recipientUserId,
        CancellationToken cancellationToken)
    {
        return dbContext.Notifications.AnyAsync(
            notification => notification.OrganisationId == organisationId
                && notification.EventId == eventId
                && notification.RecipientUserId == recipientUserId,
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Notification>> QueryAsync(
        Guid organisationId,
        Guid recipientUserId,
        NotificationQuery query,
        CancellationToken cancellationToken)
    {
        var notifications = dbContext.Notifications
            .AsNoTracking()
            .Where(notification => notification.OrganisationId == organisationId
                && notification.RecipientUserId == recipientUserId
                && !notification.IsRead);

        return await notifications
            .OrderByDescending(notification => notification.CreatedAtUtc)
            .ThenByDescending(notification => notification.Id)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task<Notification?> GetForRecipientAsync(
        Guid organisationId,
        Guid recipientUserId,
        Guid notificationId,
        CancellationToken cancellationToken)
    {
        return dbContext.Notifications.SingleOrDefaultAsync(
            notification => notification.OrganisationId == organisationId
                && notification.RecipientUserId == recipientUserId
                && notification.Id == notificationId,
            cancellationToken);
    }
}
