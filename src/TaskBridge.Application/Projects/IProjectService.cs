namespace TaskBridge.Application.Projects;

/// <summary>Provides application operations for projects.</summary>
public interface IProjectService
{
    /// <summary>Creates a project for the authenticated organisation.</summary>
    Task<ProjectResponse> CreateAsync(CreateProjectRequest request, CancellationToken cancellationToken);

    /// <summary>Updates a project status within the authenticated organisation.</summary>
    Task<ProjectResponse> UpdateStatusAsync(UpdateProjectStatusRequest request, CancellationToken cancellationToken);

    /// <summary>Gets projects assigned to a team within the authenticated organisation.</summary>
    Task<IReadOnlyList<ProjectResponse>> GetByTeamAsync(Guid teamId, CancellationToken cancellationToken);

    /// <summary>Deletes a project within the authenticated organisation.</summary>
    Task DeleteAsync(Guid projectId, CancellationToken cancellationToken);
}