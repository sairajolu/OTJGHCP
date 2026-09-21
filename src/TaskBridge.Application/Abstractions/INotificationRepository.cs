using TaskBridge.Application.Notifications;
using TaskBridge.Domain.Entities;

namespace TaskBridge.Application.Abstractions;

/// <summary>Provides organisation- and recipient-scoped notification persistence.</summary>
public interface INotificationRepository
{
    /// <summary>Stages a notification for insertion.</summary>
    Task AddAsync(Notification notification, CancellationToken cancellationToken);

    /// <summary>Checks whether a recipient already received an event.</summary>
    Task<bool> ExistsByEventIdAndRecipientAsync(
        Guid organisationId,
        Guid eventId,
        Guid recipientUserId,
        CancellationToken cancellationToken);

    /// <summary>Queries notifications for a recipient within an organisation.</summary>
    Task<IReadOnlyList<Notification>> QueryAsync(
        Guid organisationId,
        Guid recipientUserId,
        NotificationQuery query,
        CancellationToken cancellationToken);

    /// <summary>Gets a notification owned by the specified recipient and organisation.</summary>
    Task<Notification?> GetForRecipientAsync(
        Guid organisationId,
        Guid recipientUserId,
        Guid notificationId,
        CancellationToken cancellationToken);
}
