using TaskBridge.Domain.Enums;

namespace TaskBridge.Domain.Entities;

/// <summary>Represents an immutable-content tenant notification.</summary>
public sealed class Notification
{
    private Notification()
    {
    }

    private Notification(
        Guid id,
        Guid eventId,
        Guid recipientUserId,
        Guid organisationId,
        MilestoneEventType eventType,
        Guid projectId,
        Guid? milestoneId,
        string title,
        string message,
        DateTime createdAtUtc)
    {
        Id = id;
        EventId = eventId;
        RecipientUserId = recipientUserId;
        OrganisationId = organisationId;
        EventType = eventType;
        ProjectId = projectId;
        MilestoneId = milestoneId;
        Title = title;
        Message = message;
        CreatedAtUtc = createdAtUtc;
    }

    /// <summary>Gets the notification identifier.</summary>
    public Guid Id { get; private set; }

    /// <summary>Gets the idempotency event identifier.</summary>
    public Guid EventId { get; private set; }

    /// <summary>Gets the recipient user identifier.</summary>
    public Guid RecipientUserId { get; private set; }

    /// <summary>Gets the owning organisation identifier.</summary>
    public Guid OrganisationId { get; private set; }

    /// <summary>Gets the event type.</summary>
    public MilestoneEventType EventType { get; private set; }

    /// <summary>Gets the related project identifier.</summary>
    public Guid ProjectId { get; private set; }

    /// <summary>Gets the related milestone identifier.</summary>
    public Guid? MilestoneId { get; private set; }

    /// <summary>Gets the notification title.</summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>Gets the notification message.</summary>
    public string Message { get; private set; } = string.Empty;

    /// <summary>Gets whether the notification has been read.</summary>
    public bool IsRead { get; private set; }

    /// <summary>Gets the creation timestamp in UTC.</summary>
    public DateTime CreatedAtUtc { get; private set; }

    /// <summary>Gets the read timestamp in UTC, when applicable.</summary>
    public DateTime? ReadAtUtc { get; private set; }

    /// <summary>Creates a notification with immutable content.</summary>
    public static Notification Create(
        Guid eventId,
        Guid recipientUserId,
        Guid organisationId,
        MilestoneEventType eventType,
        Guid projectId,
        Guid? milestoneId,
        string title,
        string message,
        DateTime createdAtUtc)
    {
        EnsureIdentifier(eventId, nameof(eventId));
        EnsureIdentifier(recipientUserId, nameof(recipientUserId));
        EnsureIdentifier(organisationId, nameof(organisationId));
        EnsureIdentifier(projectId, nameof(projectId));
        EnsureText(title, nameof(title), 200);
        EnsureText(message, nameof(message), 2000);

        if (!Enum.IsDefined(eventType))
        {
            throw new ArgumentOutOfRangeException(nameof(eventType));
        }

        if (createdAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("The timestamp must be UTC.", nameof(createdAtUtc));
        }

        return new Notification(
            Guid.NewGuid(),
            eventId,
            recipientUserId,
            organisationId,
            eventType,
            projectId,
            milestoneId,
            title.Trim(),
            message.Trim(),
            createdAtUtc);
    }

    /// <summary>Marks the notification as read at the supplied UTC timestamp.</summary>
    public void MarkAsRead(DateTime readAtUtc)
    {
        if (readAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("The timestamp must be UTC.", nameof(readAtUtc));
        }

        if (!IsRead)
        {
            IsRead = true;
            ReadAtUtc = readAtUtc;
        }
    }

    private static void EnsureIdentifier(Guid value, string parameterName)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("A non-empty identifier is required.", parameterName);
        }
    }

    private static void EnsureText(string value, string parameterName, int maximumLength)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > maximumLength)
        {
            throw new ArgumentException("The value is required and exceeds the permitted length.", parameterName);
        }
    }
}
