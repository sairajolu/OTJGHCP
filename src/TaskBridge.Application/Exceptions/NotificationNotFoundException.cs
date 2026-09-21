namespace TaskBridge.Application.Exceptions;

/// <summary>Indicates that a notification is unavailable to the current user and organisation.</summary>
public sealed class NotificationNotFoundException : KeyNotFoundException
{
    /// <summary>Initializes the exception for a notification identifier.</summary>
    public NotificationNotFoundException(Guid notificationId)
        : base($"Notification '{notificationId}' was not found.")
    {
    }
}
