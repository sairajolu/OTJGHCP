using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using TaskBridge.Application.Abstractions;
using TaskBridge.Application.Exceptions;
using TaskBridge.Domain.Entities;
using TaskBridge.Domain.Enums;

namespace TaskBridge.Application.Notifications;

/// <summary>Coordinates tenant- and recipient-scoped notification operations.</summary>
public sealed class NotificationService : INotificationService
{
    private readonly INotificationRepository notificationRepository;
    private readonly ICurrentUserContext currentUserContext;
    private readonly IClock clock;
    private readonly ILogger<NotificationService> logger;

    /// <summary>Initializes the notification service.</summary>
    public NotificationService(
        INotificationRepository notificationRepository,
        ICurrentUserContext currentUserContext,
        IClock clock,
        ILogger<NotificationService>? logger = null)
    {
        this.notificationRepository = notificationRepository;
        this.currentUserContext = currentUserContext;
        this.clock = clock;
        this.logger = logger ?? NullLogger<NotificationService>.Instance;
    }

    /// <inheritdoc />
    public async Task CreateForRecipientsAsync(
        MilestoneChangedEvent milestoneEvent,
        IReadOnlyCollection<Guid> recipientUserIds,
        CancellationToken cancellationToken)
    {
        EnsureTenant(milestoneEvent.OrganisationId);

        foreach (var recipientUserId in recipientUserIds.Distinct())
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (recipientUserId == Guid.Empty || recipientUserId == milestoneEvent.ActorUserId)
            {
                continue;
            }

            var exists = await notificationRepository.ExistsByEventIdAndRecipientAsync(
                currentUserContext.OrganisationId,
                milestoneEvent.EventId,
                recipientUserId,
                cancellationToken);

            if (exists)
            {
                continue;
            }

            var notification = Notification.Create(
                milestoneEvent.EventId,
                recipientUserId,
                currentUserContext.OrganisationId,
                milestoneEvent.EventType,
                milestoneEvent.ProjectId,
                milestoneEvent.MilestoneId,
                GetTitle(milestoneEvent.EventType),
                GetMessage(milestoneEvent.EventType),
                clock.UtcNow);

            await notificationRepository.AddAsync(notification, cancellationToken);

            logger.LogInformation(
                "Staged notification {NotificationId} for event {EventId}, recipient {RecipientUserId}, organisation {OrganisationId}, event type {EventType}",
                notification.Id,
                notification.EventId,
                notification.RecipientUserId,
                notification.OrganisationId,
                notification.EventType);
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<NotificationResponse>> GetForUserAsync(
        Guid userId,
        NotificationQuery query,
        CancellationToken cancellationToken)
    {
        EnsureOwnUser(userId);
        ValidateQuery(query);

        var notifications = await notificationRepository.QueryAsync(
            currentUserContext.OrganisationId,
            userId,
            query,
            cancellationToken);

        return notifications.Select(ToResponse).ToArray();
    }

    /// <inheritdoc />
    public async Task MarkAsReadAsync(Guid notificationId, CancellationToken cancellationToken)
    {
        if (notificationId == Guid.Empty)
        {
            throw new ArgumentException("A notification identifier is required.", nameof(notificationId));
        }

        var notification = await notificationRepository.GetForRecipientAsync(
            currentUserContext.OrganisationId,
            currentUserContext.UserId,
            notificationId,
            cancellationToken);

        if (notification is null)
        {
            throw new NotificationNotFoundException(notificationId);
        }

        notification.MarkAsRead(clock.UtcNow);
        logger.LogInformation(
            "Marked notification {NotificationId} as read for user {UserId}, organisation {OrganisationId}",
            notificationId,
            currentUserContext.UserId,
            currentUserContext.OrganisationId);
    }

    private void EnsureTenant(Guid organisationId)
    {
        if (organisationId != currentUserContext.OrganisationId)
        {
            throw new TenantContextMismatchException();
        }
    }

    private void EnsureOwnUser(Guid userId)
    {
        if (userId == Guid.Empty || userId != currentUserContext.UserId)
        {
            throw new NotificationAccessDeniedException();
        }
    }

    private static void ValidateQuery(NotificationQuery query)
    {
        if (query.PageNumber < 1 || query.PageSize is < 1 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(query), "Page number must be positive and page size must be between 1 and 100.");
        }
    }

    private static string GetTitle(MilestoneEventType eventType)
    {
        return eventType switch
        {
            MilestoneEventType.MilestoneCreated => "Milestone created",
            MilestoneEventType.MilestoneUpdated => "Milestone updated",
            MilestoneEventType.MilestoneDeleted => "Milestone deleted",
            MilestoneEventType.MilestoneReopened => "Milestone reopened",
            _ => throw new ArgumentOutOfRangeException(nameof(eventType))
        };
    }

    private static string GetMessage(MilestoneEventType eventType)
    {
        return eventType switch
        {
            MilestoneEventType.MilestoneCreated => "A milestone was created for a project you follow.",
            MilestoneEventType.MilestoneUpdated => "A milestone was updated for a project you follow.",
            MilestoneEventType.MilestoneDeleted => "A milestone was deleted from a project you follow.",
            MilestoneEventType.MilestoneReopened => "A milestone was reopened for a project you follow.",
            _ => throw new ArgumentOutOfRangeException(nameof(eventType))
        };
    }

    private static NotificationResponse ToResponse(Notification notification)
    {
        return new NotificationResponse(
            notification.Id,
            notification.EventId,
            notification.RecipientUserId,
            notification.OrganisationId,
            notification.EventType,
            notification.ProjectId,
            notification.MilestoneId,
            notification.Title,
            notification.Message,
            notification.IsRead,
            notification.CreatedAtUtc,
            notification.ReadAtUtc);
    }
}
