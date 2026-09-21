using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using TaskBridge.Application.Abstractions;
using TaskBridge.Application.Exceptions;
using TaskBridge.Domain.Entities;

namespace TaskBridge.Application.Notifications;

/// <summary>Coordinates append-only audit persistence and tenant-scoped queries.</summary>
public sealed class AuditService : IAuditService
{
    private readonly IAuditEntryRepository auditRepository;
    private readonly ICurrentUserContext currentUserContext;
    private readonly IClock clock;
    private readonly ILogger<AuditService> logger;

    /// <summary>Initializes the audit service.</summary>
    public AuditService(
        IAuditEntryRepository auditRepository,
        ICurrentUserContext currentUserContext,
        IClock clock,
        ILogger<AuditService>? logger = null)
    {
        this.auditRepository = auditRepository;
        this.currentUserContext = currentUserContext;
        this.clock = clock;
        this.logger = logger ?? NullLogger<AuditService>.Instance;
    }

    /// <inheritdoc />
    public Task<AuditEntryResponse> AppendAsync(
        AuditEntryRequest request,
        CancellationToken cancellationToken)
    {
        return AppendCoreAsync(
            request,
            currentUserContext.UserId,
            currentUserContext.ActorIpAddress,
            clock.UtcNow,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<AuditEntryResponse> AppendForEventAsync(
        MilestoneChangedEvent milestoneEvent,
        CancellationToken cancellationToken)
    {
        if (milestoneEvent.OrganisationId != currentUserContext.OrganisationId)
        {
            throw new TenantContextMismatchException();
        }

        var request = new AuditEntryRequest(
            milestoneEvent.EventId,
            milestoneEvent.EventType,
            "Milestone",
            milestoneEvent.MilestoneId,
            milestoneEvent.ProjectId,
            milestoneEvent.MilestoneId,
            milestoneEvent.PreviousState,
            milestoneEvent.NewState,
            milestoneEvent.CorrelationId);

        return AppendCoreAsync(
            request,
            milestoneEvent.ActorUserId,
            milestoneEvent.ActorIpAddress,
            milestoneEvent.OccurredAtUtc,
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<AuditEntryResponse>> GetByProjectAsync(
        AuditEntryQuery query,
        CancellationToken cancellationToken)
    {
        ValidateQuery(query);

        var entries = await auditRepository.QueryAsync(
            currentUserContext.OrganisationId,
            query,
            cancellationToken);

        return entries.Select(ToResponse).ToArray();
    }

    private async Task<AuditEntryResponse> AppendCoreAsync(
        AuditEntryRequest request,
        Guid actorUserId,
        string? actorIpAddress,
        DateTime timestampUtc,
        CancellationToken cancellationToken)
    {
        if (actorUserId == Guid.Empty)
        {
            throw new ArgumentException("An actor identifier is required.", nameof(actorUserId));
        }

        var existing = await auditRepository.GetByEventIdAsync(
            currentUserContext.OrganisationId,
            request.EventId,
            cancellationToken);

        if (existing is not null)
        {
            return ToResponse(existing);
        }

        var nowUtc = clock.UtcNow;
        var entry = AuditEntry.Create(
            request.EventId,
            request.EventType,
            request.EntityType,
            request.EntityId,
            actorUserId,
            currentUserContext.OrganisationId,
            actorIpAddress,
            request.PreviousState,
            request.NewState,
            timestampUtc,
            request.ProjectId,
            request.MilestoneId,
            request.CorrelationId,
            nowUtc);

        await auditRepository.AddAsync(entry, cancellationToken);

        logger.LogInformation(
            "Staged audit entry {AuditEntryId} for event {EventId}, project {ProjectId}, organisation {OrganisationId}, actor {ActorUserId}, event type {EventType}",
            entry.Id,
            entry.EventId,
            entry.ProjectId,
            entry.OrganisationId,
            entry.ActorUserId,
            entry.EventType);

        return ToResponse(entry);
    }

    private static void ValidateQuery(AuditEntryQuery query)
    {
        if (query.ProjectId == Guid.Empty)
        {
            throw new ArgumentException("A project identifier is required.", nameof(query));
        }

        if (query.PageNumber < 1 || query.PageSize is < 1 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(query), "Page number must be positive and page size must be between 1 and 100.");
        }

        if (query.FromUtc is { Kind: not DateTimeKind.Utc } || query.ToUtc is { Kind: not DateTimeKind.Utc })
        {
            throw new ArgumentException("Audit date filters must be UTC.", nameof(query));
        }

        if (query.FromUtc.HasValue && query.ToUtc.HasValue && query.FromUtc > query.ToUtc)
        {
            throw new InvalidAuditDateRangeException();
        }

        if (query.FromUtc.HasValue
            && query.ToUtc.HasValue
            && query.ToUtc.Value - query.FromUtc.Value > TimeSpan.FromDays(90))
        {
            throw new ArgumentOutOfRangeException(nameof(query), "The audit date range cannot exceed 90 days.");
        }

        if (query.EventType.HasValue && !Enum.IsDefined(query.EventType.Value))
        {
            throw new ArgumentOutOfRangeException(nameof(query), "The audit event type is invalid.");
        }
    }

    private static AuditEntryResponse ToResponse(AuditEntry entry)
    {
        return new AuditEntryResponse(
            entry.Id,
            entry.EventId,
            entry.EventType,
            entry.EntityType,
            entry.EntityId,
            entry.ActorUserId,
            entry.OrganisationId,
            entry.TimestampUtc,
            entry.ProjectId,
            entry.MilestoneId,
            entry.CorrelationId);
    }
}
