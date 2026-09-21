namespace TaskBridge.Application.Notifications;

/// <summary>Handles trusted milestone events for audit and notification processing.</summary>
public interface IMilestoneEventHandler
{
    /// <summary>Stages one audit entry and recipient notifications for an event.</summary>
    Task HandleAsync(
        MilestoneChangedEvent milestoneEvent,
        CancellationToken cancellationToken);
}
