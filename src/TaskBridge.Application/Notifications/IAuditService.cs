namespace TaskBridge.Application.Notifications;

/// <summary>Provides append-only audit operations.</summary>
public interface IAuditService
{
    /// <summary>Stages an audit entry using the authenticated organisation and actor.</summary>
    Task<AuditEntryResponse> AppendAsync(
        AuditEntryRequest request,
        CancellationToken cancellationToken);

    /// <summary>Stages an audit entry from a trusted milestone event.</summary>
    Task<AuditEntryResponse> AppendForEventAsync(
        MilestoneChangedEvent milestoneEvent,
        CancellationToken cancellationToken);

    /// <summary>Gets tenant-scoped audit entries for a project.</summary>
    Task<IReadOnlyList<AuditEntryResponse>> GetByProjectAsync(
        AuditEntryQuery query,
        CancellationToken cancellationToken);
}
