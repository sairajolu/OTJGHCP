using System.Security.Claims;
using System.Net;
using TaskBridge.Application.Abstractions;

namespace TaskBridge.Api.Tenancy;

/// <summary>Resolves tenant identity from authenticated HTTP claims.</summary>
public sealed class HttpCurrentTenant : ICurrentTenant
{
    private readonly IHttpContextAccessor httpContextAccessor;

    /// <summary>Initializes the HTTP tenant context.</summary>
    public HttpCurrentTenant(IHttpContextAccessor httpContextAccessor)
    {
        this.httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc />
    public Guid OrganisationId => GetRequiredGuidClaim("organisation_id");

    /// <inheritdoc />
    public Guid UserId => GetRequiredGuidClaim(ClaimTypes.NameIdentifier);

    /// <inheritdoc />
    public string? ActorIpAddress => httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

    private Guid GetRequiredGuidClaim(string claimType)
    {
        var value = httpContextAccessor.HttpContext?.User.FindFirstValue(claimType);

        if (!Guid.TryParse(value, out var identifier) || identifier == Guid.Empty)
        {
            throw new UnauthorizedAccessException($"The authenticated request is missing a valid '{claimType}' claim.");
        }

        return identifier;
    }
}