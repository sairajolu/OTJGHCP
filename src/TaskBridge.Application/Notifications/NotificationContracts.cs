using TaskBridge.Domain.Entities;
using TaskBridge.Domain.Enums;

namespace TaskBridge.Application.Notifications;

/// <summary>Contains a trusted milestone lifecycle event.</summary>
/// <param name="EventId">The idempotency identifier assigned by the trusted producer.</param>
/// <param name="OrganisationId">The authenticated organisation owning the event.</param>
/// <param name="ActorUserId">The authenticated actor who caused the event.</param>
/// <param name="ActorIpAddress">The optional IP captured from trusted server context.</param>
/// <param name="ProjectId">The tenant-owned project identifier.</param>
/// <param name="TeamId">The tenant-owned team identifier used for recipient resolution.</param>
/// <param name="MilestoneId">The related milestone identifier.</param>
/// <param name="EventType">The milestone lifecycle event type.</param>
/// <param name="PreviousState">The redacted previous state as JSON.</param>
/// <param name="NewState">The redacted new state as JSON.</param>
/// <param name="OccurredAtUtc">The event timestamp in UTC.</param>
/// <param name="CorrelationId">The bounded correlation identifier.</param>
public sealed record MilestoneChangedEvent(
    Guid EventId,
    Guid OrganisationId,
    Guid ActorUserId,
    string? ActorIpAddress,
    Guid ProjectId,
    Guid TeamId,
    Guid MilestoneId,
    MilestoneEventType EventType,
    string? PreviousState,
    string? NewState,
    DateTime OccurredAtUtc,
    string CorrelationId);

/// <summary>Contains data required to append an audit entry from an authenticated context.</summary>
/// <param name="EventId">The idempotency identifier.</param>
/// <param name="EventType">The audit event type.</param>
/// <param name="EntityType">The bounded audited entity type.</param>
/// <param name="EntityId">The audited entity identifier.</param>
/// <param name="ProjectId">The tenant-owned project identifier.</param>
/// <param name="MilestoneId">The optional related milestone identifier.</param>
/// <param name="PreviousState">The redacted previous state as JSON.</param>
/// <param name="NewState">The redacted new state as JSON.</param>
/// <param name="CorrelationId">The bounded correlation identifier.</param>
public sealed record AuditEntryRequest(
    Guid EventId,
    MilestoneEventType EventType,
    string EntityType,
    Guid EntityId,
    Guid ProjectId,
    Guid? MilestoneId,
    string? PreviousState,
    string? NewState,
    string CorrelationId);

/// <summary>Filters a tenant-scoped audit query.</summary>
/// <param name="ProjectId">The tenant-owned project identifier.</param>
/// <param name="FromUtc">The optional inclusive UTC start time.</param>
/// <param name="ToUtc">The optional inclusive UTC end time.</param>
/// <param name="EventType">The optional event type filter.</param>
/// <param name="PageNumber">The one-based page number.</param>
/// <param name="PageSize">The bounded page size.</param>
public sealed record AuditEntryQuery(
    Guid ProjectId,
    DateTime? FromUtc,
    DateTime? ToUtc,
    MilestoneEventType? EventType,
    int PageNumber,
    int PageSize);

/// <summary>Represents an audit entry response DTO.</summary>
/// <param name="Id">The audit entry identifier.</param>
/// <param name="EventId">The source event identifier.</param>
/// <param name="EventType">The audit event type.</param>
/// <param name="EntityType">The audited entity type.</param>
/// <param name="EntityId">The audited entity identifier.</param>
/// <param name="ActorUserId">The actor identifier.</param>
/// <param name="OrganisationId">The owning organisation identifier.</param>
/// <param name="TimestampUtc">The event timestamp in UTC.</param>
/// <param name="ProjectId">The related tenant-owned project identifier.</param>
/// <param name="MilestoneId">The optional related milestone identifier.</param>
/// <param name="CorrelationId">The correlation identifier.</param>
public sealed record AuditEntryResponse(
    Guid Id,
    Guid EventId,
    MilestoneEventType EventType,
    string EntityType,
    Guid EntityId,
    Guid ActorUserId,
    Guid OrganisationId,
    DateTime TimestampUtc,
    Guid ProjectId,
    Guid? MilestoneId,
    string CorrelationId);

/// <summary>Filters notifications for one authenticated user.</summary>
/// <param name="PageNumber">The one-based page number.</param>
/// <param name="PageSize">The bounded page size.</param>
public sealed record NotificationQuery(
    int PageNumber,
    int PageSize);

/// <summary>Represents a notification response DTO.</summary>
/// <param name="Id">The notification identifier.</param>
/// <param name="EventId">The source event identifier.</param>
/// <param name="RecipientUserId">The authenticated recipient identifier.</param>
/// <param name="OrganisationId">The owning organisation identifier.</param>
/// <param name="EventType">The notification event type.</param>
/// <param name="ProjectId">The related tenant-owned project identifier.</param>
/// <param name="MilestoneId">The optional related milestone identifier.</param>
/// <param name="Title">The safe notification title.</param>
/// <param name="Message">The safe notification message.</param>
/// <param name="IsRead">Whether the notification has been read.</param>
/// <param name="CreatedAtUtc">The creation timestamp in UTC.</param>
/// <param name="ReadAtUtc">The optional read timestamp in UTC.</param>
public sealed record NotificationResponse(
    Guid Id,
    Guid EventId,
    Guid RecipientUserId,
    Guid OrganisationId,
    MilestoneEventType EventType,
    Guid ProjectId,
    Guid? MilestoneId,
    string Title,
    string Message,
    bool IsRead,
    DateTime CreatedAtUtc,
    DateTime? ReadAtUtc);
