using Microsoft.EntityFrameworkCore;
using TaskBridge.Application.Abstractions;
using TaskBridge.Domain.Entities;
using TaskBridge.Infrastructure.Persistence;

namespace TaskBridge.Infrastructure.Repositories;

/// <summary>Persists projects through EF Core with organisation-scoped queries.</summary>
public sealed class ProjectRepository : IProjectRepository
{
    private readonly TaskBridgeDbContext dbContext;

    /// <summary>Initializes the project repository.</summary>
    public ProjectRepository(TaskBridgeDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task AddAsync(Project project, CancellationToken cancellationToken)
    {
        await dbContext.Projects.AddAsync(project, cancellationToken);
    }

    /// <inheritdoc />
    public Task<Project?> GetByIdAsync(
        Guid organisationId,
        Guid projectId,
        CancellationToken cancellationToken)
    {
        return dbContext.Projects
            .SingleOrDefaultAsync(
                project => project.OrganisationId == organisationId && project.Id == projectId,
                cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Project>> GetByTeamAsync(
        Guid organisationId,
        Guid teamId,
        CancellationToken cancellationToken)
    {
        return await dbContext.Projects
            .AsNoTracking()
            .Where(project => project.OrganisationId == organisationId && project.TeamId == teamId)
            .OrderBy(project => project.Name)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public void Remove(Project project)
    {
        dbContext.Projects.Remove(project);
    }

    /// <inheritdoc />
    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}