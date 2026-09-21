using TaskBridge.Domain.Entities;

namespace TaskBridge.Application.Exceptions;

/// <summary>Indicates that a requested project status transition is not allowed.</summary>
public sealed class InvalidProjectStatusTransitionException : InvalidOperationException
{
    /// <summary>Initializes the exception for the rejected transition.</summary>
    public InvalidProjectStatusTransitionException(ProjectStatus currentStatus, ProjectStatus requestedStatus)
        : base($"Transition from '{currentStatus}' to '{requestedStatus}' is not allowed.")
    {
        CurrentStatus = currentStatus;
        RequestedStatus = requestedStatus;
    }

    /// <summary>Gets the current project status.</summary>
    public ProjectStatus CurrentStatus { get; }

    /// <summary>Gets the requested project status.</summary>
    public ProjectStatus RequestedStatus { get; }
}
