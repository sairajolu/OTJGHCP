using TaskBridge.Application.Notifications;
using TaskBridge.Domain.Entities;

namespace TaskBridge.Application.Abstractions;

/// <summary>Provides append-only, organisation-scoped audit persistence.</summary>
public interface IAuditEntryRepository
{
    /// <summary>Stages a new audit entry for insertion.</summary>
    Task AddAsync(AuditEntry entry, CancellationToken cancellationToken);

    /// <summary>Checks whether an event has already been recorded for an organisation.</summary>
    Task<bool> ExistsByEventIdAsync(Guid organisationId, Guid eventId, CancellationToken cancellationToken);

    /// <summary>Gets an existing event entry for idempotent processing.</summary>
    Task<AuditEntry?> GetByEventIdAsync(
        Guid organisationId,
        Guid eventId,
        CancellationToken cancellationToken);

    /// <summary>Queries audit entries for an organisation-scoped project.</summary>
    Task<IReadOnlyList<AuditEntry>> QueryAsync(
        Guid organisationId,
        AuditEntryQuery query,
        CancellationToken cancellationToken);
}
