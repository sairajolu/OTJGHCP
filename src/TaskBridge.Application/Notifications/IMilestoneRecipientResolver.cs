namespace TaskBridge.Application.Notifications;

/// <summary>Resolves active team members eligible for milestone notifications.</summary>
public interface IMilestoneRecipientResolver
{
    /// <summary>Gets recipients within the supplied organisation.</summary>
    Task<IReadOnlyCollection<Guid>> ResolveAsync(
        Guid organisationId,
        Guid teamId,
        Guid actorUserId,
        CancellationToken cancellationToken);
}
