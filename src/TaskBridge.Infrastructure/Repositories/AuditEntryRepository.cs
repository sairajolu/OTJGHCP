using Microsoft.EntityFrameworkCore;
using TaskBridge.Application.Abstractions;
using TaskBridge.Application.Notifications;
using TaskBridge.Domain.Entities;
using TaskBridge.Infrastructure.Persistence;

namespace TaskBridge.Infrastructure.Repositories;

/// <summary>Persists append-only audit entries with organisation-scoped queries.</summary>
public sealed class AuditEntryRepository : IAuditEntryRepository
{
    private readonly TaskBridgeDbContext dbContext;

    /// <summary>Initializes the audit entry repository.</summary>
    public AuditEntryRepository(TaskBridgeDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task AddAsync(AuditEntry entry, CancellationToken cancellationToken)
    {
        await dbContext.AuditEntries.AddAsync(entry, cancellationToken);
    }

    /// <inheritdoc />
    public Task<bool> ExistsByEventIdAsync(
        Guid organisationId,
        Guid eventId,
        CancellationToken cancellationToken)
    {
        return dbContext.AuditEntries.AnyAsync(
            entry => entry.OrganisationId == organisationId && entry.EventId == eventId,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<AuditEntry?> GetByEventIdAsync(
        Guid organisationId,
        Guid eventId,
        CancellationToken cancellationToken)
    {
        return dbContext.AuditEntries
            .AsNoTracking()
            .SingleOrDefaultAsync(
                entry => entry.OrganisationId == organisationId && entry.EventId == eventId,
                cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<AuditEntry>> QueryAsync(
        Guid organisationId,
        AuditEntryQuery query,
        CancellationToken cancellationToken)
    {
        var entries = dbContext.AuditEntries
            .AsNoTracking()
            .Where(entry => entry.OrganisationId == organisationId && entry.ProjectId == query.ProjectId);

        if (query.FromUtc.HasValue)
        {
            entries = entries.Where(entry => entry.TimestampUtc >= query.FromUtc.Value);
        }

        if (query.ToUtc.HasValue)
        {
            entries = entries.Where(entry => entry.TimestampUtc <= query.ToUtc.Value);
        }

        if (query.EventType.HasValue)
        {
            entries = entries.Where(entry => entry.EventType == query.EventType.Value);
        }

        return await entries
            .OrderByDescending(entry => entry.TimestampUtc)
            .ThenByDescending(entry => entry.Id)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);
    }
}
