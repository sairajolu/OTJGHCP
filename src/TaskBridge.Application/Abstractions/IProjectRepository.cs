using TaskBridge.Domain.Entities;

namespace TaskBridge.Application.Abstractions;

/// <summary>Provides organisation-scoped persistence operations for projects.</summary>
public interface IProjectRepository
{
    /// <summary>Adds a project to the current unit of work.</summary>
    Task AddAsync(Project project, CancellationToken cancellationToken);

    /// <summary>Gets a tracked project in the specified organisation.</summary>
    Task<Project?> GetByIdAsync(Guid organisationId, Guid projectId, CancellationToken cancellationToken);

    /// <summary>Gets projects for a team within the specified organisation.</summary>
    Task<IReadOnlyList<Project>> GetByTeamAsync(Guid organisationId, Guid teamId, CancellationToken cancellationToken);

    /// <summary>Removes a project from the current unit of work.</summary>
    void Remove(Project project);

    /// <summary>Commits the current unit of work.</summary>
    Task SaveChangesAsync(CancellationToken cancellationToken);
}