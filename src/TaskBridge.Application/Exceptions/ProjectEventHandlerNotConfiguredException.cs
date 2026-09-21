namespace TaskBridge.Application.Exceptions;

/// <summary>Indicates that Project event processing is not configured.</summary>
public sealed class ProjectEventHandlerNotConfiguredException : InvalidOperationException
{
    /// <summary>Initializes the configuration exception.</summary>
    public ProjectEventHandlerNotConfiguredException()
        : base("Project event handling is not configured.")
    {
    }
}
