namespace TaskBridge.Application.Exceptions;

/// <summary>Indicates that a project was not found within the current organisation.</summary>
public sealed class ProjectNotFoundException : KeyNotFoundException
{
    /// <summary>Initializes the exception for the specified project.</summary>
    public ProjectNotFoundException(Guid projectId)
        : base($"Project '{projectId}' was not found.")
    {
    }
}