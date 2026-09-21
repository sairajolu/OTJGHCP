namespace TaskBridge.Application.Abstractions;

/// <summary>Commits the current application transaction.</summary>
public interface IUnitOfWork
{
    /// <summary>Commits staged changes asynchronously.</summary>
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
