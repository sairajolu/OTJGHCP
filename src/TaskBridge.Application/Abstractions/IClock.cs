namespace TaskBridge.Application.Abstractions;

/// <summary>Provides the current UTC time to application services.</summary>
public interface IClock
{
    /// <summary>Gets the current UTC timestamp.</summary>
    DateTime UtcNow { get; }
}