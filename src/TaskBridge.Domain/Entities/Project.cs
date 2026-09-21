namespace TaskBridge.Domain.Entities;

/// <summary>
/// Represents a project owned by an organisation and assigned to a team.
/// </summary>
public sealed class Project
{
    private Project()
    {
    }

    private Project(
        Guid id,
        Guid organisationId,
        Guid teamId,
        string name,
        string? description,
        DateTime createdAtUtc)
    {
        Id = id;
        OrganisationId = organisationId;
        TeamId = teamId;
        Name = name;
        Description = description;
        Status = ProjectStatus.Draft;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
    }

    /// <summary>Gets the project identifier.</summary>
    public Guid Id { get; private set; }

    /// <summary>Gets the organisation that owns the project.</summary>
    public Guid OrganisationId { get; private set; }

    /// <summary>Gets the team assigned to the project.</summary>
    public Guid TeamId { get; private set; }

    /// <summary>Gets the project name.</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Gets the optional project description.</summary>
    public string? Description { get; private set; }

    /// <summary>Gets the current project status.</summary>
    public ProjectStatus Status { get; private set; }

    /// <summary>Gets the creation timestamp in UTC.</summary>
    public DateTime CreatedAtUtc { get; private set; }

    /// <summary>Gets the last update timestamp in UTC.</summary>
    public DateTime UpdatedAtUtc { get; private set; }

    /// <summary>Creates a new project in draft status.</summary>
    public static Project Create(
        Guid organisationId,
        Guid teamId,
        string name,
        string? description,
        DateTime createdAtUtc)
    {
        return new Project(Guid.NewGuid(), organisationId, teamId, name, description, createdAtUtc);
    }

    /// <summary>Updates the project status and modification timestamp.</summary>
    public void UpdateStatus(ProjectStatus status, DateTime updatedAtUtc)
    {
        Status = status;
        UpdatedAtUtc = updatedAtUtc;
    }
}