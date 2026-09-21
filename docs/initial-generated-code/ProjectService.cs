using FluentValidation;
using TaskBridge.Application.Abstractions;
using TaskBridge.Application.Exceptions;
using TaskBridge.Domain.Entities;

namespace TaskBridge.Application.Projects;

/// <summary>Coordinates project business operations for the authenticated organisation.</summary>
public sealed class ProjectService : IProjectService
{
    private readonly IProjectRepository projectRepository;
    private readonly ICurrentTenant currentTenant;
    private readonly IClock clock;
    private readonly IValidator<CreateProjectRequest> createValidator;
    private readonly IValidator<UpdateProjectStatusRequest> updateStatusValidator;

    /// <summary>Initializes the project service.</summary>
    public ProjectService(
        IProjectRepository projectRepository,
        ICurrentTenant currentTenant,
        IClock clock,
        IValidator<CreateProjectRequest> createValidator,
        IValidator<UpdateProjectStatusRequest> updateStatusValidator)
    {
        this.projectRepository = projectRepository;
        this.currentTenant = currentTenant;
        this.clock = clock;
        this.createValidator = createValidator;
        this.updateStatusValidator = updateStatusValidator;
    }

    /// <inheritdoc />
    public async Task<ProjectResponse> CreateAsync(
        CreateProjectRequest request,
        CancellationToken cancellationToken)
    {
        await createValidator.ValidateAndThrowAsync(request, cancellationToken);

        var project = Project.Create(
            currentTenant.OrganisationId,
            request.TeamId,
            request.Name.Trim(),
            request.Description?.Trim(),
            clock.UtcNow);

        await projectRepository.AddAsync(project, cancellationToken);
        await projectRepository.SaveChangesAsync(cancellationToken);

        return ToResponse(project);
    }

    /// <inheritdoc />
    public async Task<ProjectResponse> UpdateStatusAsync(
        UpdateProjectStatusRequest request,
        CancellationToken cancellationToken)
    {
        await updateStatusValidator.ValidateAndThrowAsync(request, cancellationToken);

        var project = await projectRepository.GetByIdAsync(
            currentTenant.OrganisationId,
            request.ProjectId,
            cancellationToken);

        if (project is null)
        {
            throw new ProjectNotFoundException(request.ProjectId);
        }

        project.UpdateStatus(request.Status, clock.UtcNow);
        await projectRepository.SaveChangesAsync(cancellationToken);

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
            currentTenant.OrganisationId,
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
            currentTenant.OrganisationId,
            projectId,
            cancellationToken);

        if (project is null)
        {
            throw new ProjectNotFoundException(projectId);
        }

        projectRepository.Remove(project);
        await projectRepository.SaveChangesAsync(cancellationToken);
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
}