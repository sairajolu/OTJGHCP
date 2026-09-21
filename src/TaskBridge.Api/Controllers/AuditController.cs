using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskBridge.Application.Exceptions;
using TaskBridge.Application.Notifications;
using TaskBridge.Domain.Enums;

namespace TaskBridge.Api.Controllers;

/// <summary>Provides tenant-scoped audit endpoints.</summary>
[ApiController]
[Route("audit")]
[Authorize]
public sealed class AuditController : ControllerBase
{
    private readonly IAuditService auditService;

    /// <summary>Initializes the audit controller.</summary>
    public AuditController(IAuditService auditService)
    {
        this.auditService = auditService;
    }

    /// <summary>Appends an audit entry from an authorized internal service request.</summary>
    [HttpPost]
    [Authorize(Policy = "InternalService")]
    [ProducesResponseType(typeof(AuditEntryResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AuditEntryResponse>> PostAsync(
        [FromBody] AppendAuditRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await auditService.AppendAsync(
                new AuditEntryRequest(
                    request.EventId,
                    request.EventType,
                    request.EntityType,
                    request.EntityId,
                    request.ProjectId,
                    request.MilestoneId,
                    request.PreviousState,
                    request.NewState,
                    request.CorrelationId),
                cancellationToken);

            return CreatedAtAction(
                nameof(GetByProjectAsync),
                new { projectId = result.ProjectId },
                result);
        }
        catch (ArgumentException)
        {
            return BadRequest(new ProblemDetails { Title = "Invalid audit request", Detail = "The audit request is invalid." });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    /// <summary>Gets audit entries for a project within the authenticated organisation.</summary>
    [HttpGet("{projectId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<AuditEntryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<AuditEntryResponse>>> GetByProjectAsync(
        Guid projectId,
        [FromQuery] AuditQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await auditService.GetByProjectAsync(
                new AuditEntryQuery(
                    projectId,
                    parameters.From,
                    parameters.To,
                    parameters.EventType,
                    parameters.PageNumber,
                    parameters.PageSize),
                cancellationToken);

            return Ok(result);
        }
        catch (ArgumentException)
        {
            return BadRequest(new ProblemDetails { Title = "Invalid audit query", Detail = "The audit query is invalid." });
        }
        catch (UnauthorizedAccessException)
        {
            return NotFound();
        }
    }
}

/// <summary>Request body for an internal audit append.</summary>
/// <param name="EventId">The trusted event idempotency identifier.</param>
/// <param name="EventType">The validated event type.</param>
/// <param name="EntityType">The bounded audited entity type.</param>
/// <param name="EntityId">The audited entity identifier.</param>
/// <param name="ProjectId">The tenant-owned project identifier.</param>
/// <param name="MilestoneId">The optional related milestone identifier.</param>
/// <param name="PreviousState">The redacted previous state as JSON.</param>
/// <param name="NewState">The redacted new state as JSON.</param>
/// <param name="CorrelationId">The bounded correlation identifier.</param>
public sealed record AppendAuditRequest(
    Guid EventId,
    MilestoneEventType EventType,
    string EntityType,
    Guid EntityId,
    Guid ProjectId,
    Guid? MilestoneId,
    string? PreviousState,
    string? NewState,
    string CorrelationId);

/// <summary>Bounded audit query parameters.</summary>
public sealed class AuditQueryParameters
{
    /// <summary>Gets the inclusive UTC start filter.</summary>
    public DateTime? From { get; init; }

    /// <summary>Gets the inclusive UTC end filter.</summary>
    public DateTime? To { get; init; }

    /// <summary>Gets the optional event type filter.</summary>
    public MilestoneEventType? EventType { get; init; }

    /// <summary>Gets the one-based page number.</summary>
    public int PageNumber { get; init; } = 1;

    /// <summary>Gets the bounded page size.</summary>
    public int PageSize { get; init; } = 50;
}
