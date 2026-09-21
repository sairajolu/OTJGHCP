using System.Text.Json;
using System.Net;
using TaskBridge.Domain.Enums;

namespace TaskBridge.Domain.Entities;

/// <summary>Represents an immutable tenant-scoped audit entry.</summary>
public sealed class AuditEntry
{
    private AuditEntry()
    {
    }

    private AuditEntry(
        Guid id,
        Guid eventId,
        MilestoneEventType eventType,
        string entityType,
        Guid entityId,
        Guid actorUserId,
        Guid organisationId,
        string? actorIpAddress,
        string? previousState,
        string? newState,
        DateTime timestampUtc,
        Guid projectId,
        Guid? milestoneId,
        string correlationId,
        DateTime createdAtUtc)
    {
        Id = id;
        EventId = eventId;
        EventType = eventType;
        EntityType = entityType;
        EntityId = entityId;
        ActorUserId = actorUserId;
        OrganisationId = organisationId;
        ActorIpAddress = actorIpAddress;
        PreviousState = previousState;
        NewState = newState;
        TimestampUtc = timestampUtc;
        ProjectId = projectId;
        MilestoneId = milestoneId;
        CorrelationId = correlationId;
        CreatedAtUtc = createdAtUtc;
    }

    /// <summary>Gets the audit entry identifier.</summary>
    public Guid Id { get; private set; }

    /// <summary>Gets the idempotency event identifier.</summary>
    public Guid EventId { get; private set; }

    /// <summary>Gets the event type.</summary>
    public MilestoneEventType EventType { get; private set; }

    /// <summary>Gets the audited entity type.</summary>
    public string EntityType { get; private set; } = string.Empty;

    /// <summary>Gets the audited entity identifier.</summary>
    public Guid EntityId { get; private set; }

    /// <summary>Gets the actor identifier.</summary>
    public Guid ActorUserId { get; private set; }

    /// <summary>Gets the owning organisation identifier.</summary>
    public Guid OrganisationId { get; private set; }

    /// <summary>Gets the actor IP address captured from trusted server context.</summary>
    public string? ActorIpAddress { get; private set; }

    /// <summary>Gets the previous state as redacted JSON.</summary>
    public string? PreviousState { get; private set; }

    /// <summary>Gets the new state as redacted JSON.</summary>
    public string? NewState { get; private set; }

    /// <summary>Gets the event timestamp in UTC.</summary>
    public DateTime TimestampUtc { get; private set; }

    /// <summary>Gets the related project identifier.</summary>
    public Guid ProjectId { get; private set; }

    /// <summary>Gets the related milestone identifier.</summary>
    public Guid? MilestoneId { get; private set; }

    /// <summary>Gets the correlation identifier.</summary>
    public string CorrelationId { get; private set; } = string.Empty;

    /// <summary>Gets the persistence creation timestamp in UTC.</summary>
    public DateTime CreatedAtUtc { get; private set; }

    /// <summary>Creates an immutable audit entry.</summary>
    public static AuditEntry Create(
        Guid eventId,
        MilestoneEventType eventType,
        string entityType,
        Guid entityId,
        Guid actorUserId,
        Guid organisationId,
        string? actorIpAddress,
        string? previousState,
        string? newState,
        DateTime timestampUtc,
        Guid projectId,
        Guid? milestoneId,
        string correlationId,
        DateTime createdAtUtc)
    {
        EnsureIdentifier(eventId, nameof(eventId));
        EnsureIdentifier(entityId, nameof(entityId));
        EnsureIdentifier(actorUserId, nameof(actorUserId));
        EnsureIdentifier(organisationId, nameof(organisationId));
        EnsureIdentifier(projectId, nameof(projectId));
        actorIpAddress = NormalizeIpAddress(actorIpAddress);
        EnsureText(entityType, nameof(entityType), 100);
        EnsureText(correlationId, nameof(correlationId), 100);
        EnsureUtc(timestampUtc, nameof(timestampUtc));
        EnsureUtc(createdAtUtc, nameof(createdAtUtc));
        EnsureJson(previousState, nameof(previousState));
        EnsureJson(newState, nameof(newState));

        if (!Enum.IsDefined(eventType))
        {
            throw new ArgumentOutOfRangeException(nameof(eventType));
        }

        return new AuditEntry(
            Guid.NewGuid(),
            eventId,
            eventType,
            entityType.Trim(),
            entityId,
            actorUserId,
            organisationId,
            actorIpAddress,
            previousState,
            newState,
            timestampUtc,
            projectId,
            milestoneId,
            correlationId.Trim(),
            createdAtUtc);
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

    private static void EnsureJson(string? value, string parameterName)
    {
        if (value is null)
        {
            return;
        }

        if (value.Length > 32000)
        {
            throw new ArgumentException("The snapshot exceeds the permitted length.", parameterName);
        }

        try
        {
            using var document = JsonDocument.Parse(value);
            if (document.RootElement.ValueKind is not JsonValueKind.Object)
            {
                throw new ArgumentException("The snapshot must be a JSON object.", parameterName);
            }
        }
        catch (JsonException exception)
        {
            throw new ArgumentException("The snapshot must contain valid JSON.", parameterName, exception);
        }
    }

    private static void EnsureUtc(DateTime value, string parameterName)
    {
        if (value.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("The timestamp must be UTC.", parameterName);
        }
    }

    private static string? NormalizeIpAddress(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (!IPAddress.TryParse(value, out var address))
        {
            throw new ArgumentException("The actor IP address is invalid.", nameof(value));
        }

        return address.ToString();
    }
}
