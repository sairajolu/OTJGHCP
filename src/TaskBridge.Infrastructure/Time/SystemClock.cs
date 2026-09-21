using TaskBridge.Application.Abstractions;

namespace TaskBridge.Infrastructure.Time;

/// <summary>Provides the system UTC clock.</summary>
public sealed class SystemClock : IClock
{
    /// <inheritdoc />
    public DateTime UtcNow => DateTime.UtcNow;
}