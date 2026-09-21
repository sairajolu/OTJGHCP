namespace TaskBridge.Domain.Entities;

/// <summary>Defines the lifecycle states available to a project.</summary>
public enum ProjectStatus
{
    Draft = 0,
    Active = 1,
    Completed = 2,
    Archived = 3
}