using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskBridge.Application.Exceptions;
using TaskBridge.Application.Notifications;

namespace TaskBridge.Api.Controllers;

/// <summary>Provides authenticated user notification endpoints.</summary>
[ApiController]
[Route("notifications")]
[Authorize]
public sealed class NotificationsController : ControllerBase
{
    private readonly INotificationService notificationService;

    /// <summary>Initializes the notification controller.</summary>
    public NotificationsController(INotificationService notificationService)
    {
        this.notificationService = notificationService;
    }

    /// <summary>Gets unread notifications for the authenticated user.</summary>
    [HttpGet("{userId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<NotificationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<NotificationResponse>>> GetAsync(
        Guid userId,
        [FromQuery] NotificationQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await notificationService.GetForUserAsync(
                userId,
                new NotificationQuery(parameters.PageNumber, parameters.PageSize),
                cancellationToken);

            return Ok(result);
        }
        catch (NotificationAccessDeniedException)
        {
            return NotFound();
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ProblemDetails { Title = "Invalid notification query", Detail = exception.Message });
        }
    }

    /// <summary>Marks one notification as read for the authenticated user.</summary>
    [HttpPatch("{id:guid}/read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsReadAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            await notificationService.MarkAsReadAsync(id, cancellationToken);
            return NoContent();
        }
        catch (NotificationNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ProblemDetails { Title = "Invalid notification identifier", Detail = exception.Message });
        }
    }
}

/// <summary>Bounded notification query parameters.</summary>
public sealed class NotificationQueryParameters
{
    /// <summary>Gets the one-based page number.</summary>
    public int PageNumber { get; init; } = 1;

    /// <summary>Gets the bounded page size.</summary>
    public int PageSize { get; init; } = 50;
}
