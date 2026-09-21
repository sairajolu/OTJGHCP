namespace TaskBridge.Application.Notifications;

/// <summary>Provides tenant- and recipient-scoped notification operations.</summary>
public interface INotificationService
{
    /// <summary>Stages notifications for the supplied trusted event recipients.</summary>
    Task CreateForRecipientsAsync(
        MilestoneChangedEvent milestoneEvent,
        IReadOnlyCollection<Guid> recipientUserIds,
        CancellationToken cancellationToken);

    /// <summary>Gets notifications for the authenticated user.</summary>
    Task<IReadOnlyList<NotificationResponse>> GetForUserAsync(
        Guid userId,
        NotificationQuery query,
        CancellationToken cancellationToken);

    /// <summary>Marks an authenticated user's notification as read.</summary>
    Task MarkAsReadAsync(Guid notificationId, CancellationToken cancellationToken);
}
