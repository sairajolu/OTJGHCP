namespace TaskBridge.Application.Exceptions;

/// <summary>Indicates that a user attempted to access another user's notifications.</summary>
public sealed class NotificationAccessDeniedException : UnauthorizedAccessException
{
    /// <summary>Initializes the exception for an unauthorized notification access.</summary>
    public NotificationAccessDeniedException()
        : base("The authenticated user may access only their own notifications.")
    {
    }
}
