using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text.Json;
using TaskBridge.Application.Abstractions;
using TaskBridge.Application.Exceptions;
using TaskBridge.Application.Notifications;
using TaskBridge.Domain.Entities;

namespace TaskBridge.Application.Projects;

/// <summary>Coordinates project business operations for the authenticated organisation.</summary>
public sealed class ProjectService : IProjectService
{
    private readonly IProjectRepository projectRepository;
    private readonly ICurrentUserContext currentUserContext;
    private readonly IClock clock;
    private readonly IValidator<CreateProjectRequest> createValidator;
    private readonly IValidator<UpdateProjectStatusRequest> updateStatusValidator;
    private readonly ILogger<ProjectService> logger;
    private readonly IMilestoneEventHandler? milestoneEventHandler;

    /// <summary>Initializes the project service.</summary>
    public ProjectService(
        IProjectRepository projectRepository,
        ICurrentUserContext currentUserContext,
        IClock clock,
        IValidator<CreateProjectRequest> createValidator,
        IValidator<UpdateProjectStatusRequest> updateStatusValidator,
        ILogger<ProjectService>? logger = null,
        IMilestoneEventHandler? milestoneEventHandler = null)
    {
        this.projectRepository = projectRepository;
        this.currentUserContext = currentUserContext;
        this.clock = clock;
        this.createValidator = createValidator;
        this.updateStatusValidator = updateStatusValidator;
        this.logger = logger ?? NullLogger<ProjectService>.Instance;
        this.milestoneEventHandler = milestoneEventHandler;
    }

    /// <inheritdoc />
    public async Task<ProjectResponse> CreateAsync(
        CreateProjectRequest request,
        CancellationToken cancellationToken)
    {
        await createValidator.ValidateAndThrowAsync(request, cancellationToken);

        logger.LogInformation(
            "Creating project for organisation {OrganisationId}, actor {ActorUserId}, team {TeamId}",
            currentUserContext.OrganisationId,
            currentUserContext.UserId,
            request.TeamId);

        var project = Project.Create(
            currentUserContext.OrganisationId,
            request.TeamId,
            request.Name.Trim(),
            request.Description?.Trim(),
            clock.UtcNow);

        await projectRepository.AddAsync(project, cancellationToken);
        await HandleProjectEventAsync(
            project,
            Domain.Enums.MilestoneEventType.MilestoneCreated,
            null,
            project,
            cancellationToken);

        return ToResponse(project);
    }

    /// <inheritdoc />
    public async Task<ProjectResponse> UpdateStatusAsync(
        UpdateProjectStatusRequest request,
        CancellationToken cancellationToken)
    {
        await updateStatusValidator.ValidateAndThrowAsync(request, cancellationToken);

        var project = await projectRepository.GetByIdAsync(
            currentUserContext.OrganisationId,
            request.ProjectId,
            cancellationToken);

        if (project is null)
        {
            logger.LogWarning(
                "Project {ProjectId} was not found for organisation {OrganisationId}",
                request.ProjectId,
                currentUserContext.OrganisationId);
            throw new ProjectNotFoundException(request.ProjectId);
        }

        var previousStatus = project.Status;
        EnsureValidStatusTransition(previousStatus, request.Status);
        var previousState = SerializeState(project);
        project.UpdateStatus(request.Status, clock.UtcNow);
        await HandleProjectEventAsync(
            project,
            previousStatus == ProjectStatus.Completed && request.Status == ProjectStatus.Active
                ? Domain.Enums.MilestoneEventType.MilestoneReopened
                : Domain.Enums.MilestoneEventType.MilestoneUpdated,
            previousState,
            project,
            cancellationToken);

        logger.LogInformation(
            "Updated project {ProjectId} status from {PreviousStatus} to {NewStatus} for organisation {OrganisationId}, actor {ActorUserId}",
            project.Id,
            previousStatus,
            request.Status,
            currentUserContext.OrganisationId,
            currentUserContext.UserId);

        return ToResponse(project);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ProjectResponse>> GetByTeamAsync(
        Guid teamId,
        CancellationToken cancellationToken)
    {
        if (teamId == Guid.Empty)
        {
            throw new ArgumentException("A team identifier is required.", nameof(teamId));
        }

        var projects = await projectRepository.GetByTeamAsync(
            currentUserContext.OrganisationId,
            teamId,
            cancellationToken);

        return projects.Select(ToResponse).ToArray();
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Guid projectId, CancellationToken cancellationToken)
    {
        if (projectId == Guid.Empty)
        {
            throw new ArgumentException("A project identifier is required.", nameof(projectId));
        }

        var project = await projectRepository.GetByIdAsync(
            currentUserContext.OrganisationId,
            projectId,
            cancellationToken);

        if (project is null)
        {
            logger.LogWarning(
                "Project {ProjectId} was not found for organisation {OrganisationId}",
                projectId,
                currentUserContext.OrganisationId);
            throw new ProjectNotFoundException(projectId);
        }

        var previousState = SerializeState(project);
        projectRepository.Remove(project);
        await HandleProjectEventAsync(
            project,
            Domain.Enums.MilestoneEventType.MilestoneDeleted,
            previousState,
            null,
            cancellationToken);

        logger.LogInformation(
            "Deleted project {ProjectId} for organisation {OrganisationId}, actor {ActorUserId}",
            projectId,
            currentUserContext.OrganisationId,
            currentUserContext.UserId);
    }

    private static void EnsureValidStatusTransition(
        ProjectStatus currentStatus,
        ProjectStatus requestedStatus)
    {
        var isAllowed = (currentStatus, requestedStatus) switch
        {
            (ProjectStatus.Draft, ProjectStatus.Active) => true,
            (ProjectStatus.Draft, ProjectStatus.Archived) => true,
            (ProjectStatus.Active, ProjectStatus.Completed) => true,
            (ProjectStatus.Active, ProjectStatus.Archived) => true,
            (ProjectStatus.Completed, ProjectStatus.Active) => true,
            (ProjectStatus.Completed, ProjectStatus.Archived) => true,
            _ => false
        };

        if (!isAllowed)
        {
            throw new InvalidProjectStatusTransitionException(currentStatus, requestedStatus);
        }
    }

    private static ProjectResponse ToResponse(Project project)
    {
        return new ProjectResponse(
            project.Id,
            project.OrganisationId,
            project.TeamId,
            project.Name,
            project.Description,
            project.Status,
            project.CreatedAtUtc,
            project.UpdatedAtUtc);
    }

    private async Task HandleProjectEventAsync(
        Project project,
        Domain.Enums.MilestoneEventType eventType,
        string? previousState,
        Project? newState,
        CancellationToken cancellationToken)
    {
        if (milestoneEventHandler is null)
        {
            logger.LogWarning(
                "Project event handler is not configured for project {ProjectId} and organisation {OrganisationId}",
                project.Id,
                currentUserContext.OrganisationId);
            throw new ProjectEventHandlerNotConfiguredException();
        }

        await milestoneEventHandler.HandleAsync(
            new MilestoneChangedEvent(
                Guid.NewGuid(),
                currentUserContext.OrganisationId,
                currentUserContext.UserId,
                currentUserContext.ActorIpAddress,
                project.Id,
                project.TeamId,
                project.Id,
                eventType,
                previousState,
                newState is null ? null : SerializeState(newState),
                clock.UtcNow,
                Guid.NewGuid().ToString("N")),
            cancellationToken);
    }

    private static string SerializeState(Project project)
    {
        return JsonSerializer.Serialize(new
        {
            project.Id,
            project.OrganisationId,
            project.TeamId,
            project.Name,
            project.Description,
            project.Status,
            project.CreatedAtUtc,
            project.UpdatedAtUtc
        });
    }
}