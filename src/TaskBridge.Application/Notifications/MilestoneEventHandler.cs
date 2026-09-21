using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using TaskBridge.Application.Abstractions;
using TaskBridge.Application.Exceptions;

namespace TaskBridge.Application.Notifications;

/// <summary>Processes milestone events through audit and notification abstractions.</summary>
public sealed class MilestoneEventHandler : IMilestoneEventHandler
{
    private readonly IAuditService auditService;
    private readonly INotificationService notificationService;
    private readonly IMilestoneRecipientResolver recipientResolver;
    private readonly ICurrentUserContext currentUserContext;
    private readonly IUnitOfWork unitOfWork;
    private readonly ILogger<MilestoneEventHandler> logger;

    /// <summary>Initializes the milestone event handler.</summary>
    public MilestoneEventHandler(
        IAuditService auditService,
        INotificationService notificationService,
        IMilestoneRecipientResolver recipientResolver,
        ICurrentUserContext currentUserContext,
        IUnitOfWork unitOfWork,
        ILogger<MilestoneEventHandler>? logger = null)
    {
        this.auditService = auditService;
        this.notificationService = notificationService;
        this.recipientResolver = recipientResolver;
        this.currentUserContext = currentUserContext;
        this.unitOfWork = unitOfWork;
        this.logger = logger ?? NullLogger<MilestoneEventHandler>.Instance;
    }

    /// <inheritdoc />
    public async Task HandleAsync(
        MilestoneChangedEvent milestoneEvent,
        CancellationToken cancellationToken)
    {
        if (milestoneEvent.OrganisationId != currentUserContext.OrganisationId)
        {
            throw new TenantContextMismatchException();
        }

        await auditService.AppendForEventAsync(milestoneEvent, cancellationToken);

        var recipients = await recipientResolver.ResolveAsync(
            milestoneEvent.OrganisationId,
            milestoneEvent.TeamId,
            milestoneEvent.ActorUserId,
            cancellationToken);

        await notificationService.CreateForRecipientsAsync(
            milestoneEvent,
            recipients,
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Handled milestone event {EventId} for project {ProjectId}, organisation {OrganisationId}, event type {EventType}",
            milestoneEvent.EventId,
            milestoneEvent.ProjectId,
            milestoneEvent.OrganisationId,
            milestoneEvent.EventType);
    }
}
