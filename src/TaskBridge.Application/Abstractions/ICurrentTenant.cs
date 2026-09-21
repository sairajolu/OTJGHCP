namespace TaskBridge.Application.Abstractions;

/// <summary>Provides the authenticated organisation and actor for the current request.</summary>
/// <remarks>
/// Retained as a compatibility alias for existing callers. New application
/// code should depend on <see cref="ICurrentUserContext"/>.
/// </remarks>
public interface ICurrentTenant : ICurrentUserContext
{
}