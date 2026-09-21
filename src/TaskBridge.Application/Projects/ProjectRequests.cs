using TaskBridge.Domain.Entities;

namespace TaskBridge.Application.Projects;

/// <summary>Contains the input needed to create a project.</summary>
/// <param name="TeamId">The team identifier; organisation ownership comes from authenticated context.</param>
/// <param name="Name">The project name.</param>
/// <param name="Description">The optional project description.</param>
public sealed record CreateProjectRequest(Guid TeamId, string Name, string? Description);

/// <summary>Contains the input needed to update a project's status.</summary>
/// <param name="ProjectId">The project identifier.</param>
/// <param name="Status">The requested validated status transition.</param>
public sealed record UpdateProjectStatusRequest(Guid ProjectId, ProjectStatus Status);

/// <summary>Represents the public project response.</summary>
/// <param name="Id">The project identifier.</param>
/// <param name="OrganisationId">The owning organisation identifier.</param>
/// <param name="TeamId">The assigned team identifier.</param>
/// <param name="Name">The project name.</param>
/// <param name="Description">The optional project description.</param>
/// <param name="Status">The current project status.</param>
/// <param name="CreatedAtUtc">The creation timestamp in UTC.</param>
/// <param name="UpdatedAtUtc">The last update timestamp in UTC.</param>
public sealed record ProjectResponse(
    Guid Id,
    Guid OrganisationId,
    Guid TeamId,
    string Name,
    string? Description,
    ProjectStatus Status,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);