namespace TaskBridge.Application.Abstractions;

/// <summary>Provides identity from the authenticated request context.</summary>
public interface ICurrentUserContext
{
    /// <summary>Gets the authenticated organisation identifier.</summary>
    Guid OrganisationId { get; }

    /// <summary>Gets the authenticated actor identifier.</summary>
    Guid UserId { get; }

    /// <summary>Gets the trusted actor IP address, when available.</summary>
    string? ActorIpAddress => null;
}
