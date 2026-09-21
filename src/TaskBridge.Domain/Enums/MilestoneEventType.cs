namespace TaskBridge.Domain.Enums;

/// <summary>Identifies a milestone lifecycle event.</summary>
public enum MilestoneEventType
{
    /// <summary>A milestone was created.</summary>
    MilestoneCreated = 1,

    /// <summary>A milestone was updated.</summary>
    MilestoneUpdated = 2,

    /// <summary>A milestone was deleted.</summary>
    MilestoneDeleted = 3,

    /// <summary>A milestone was reopened.</summary>
    MilestoneReopened = 4
}
