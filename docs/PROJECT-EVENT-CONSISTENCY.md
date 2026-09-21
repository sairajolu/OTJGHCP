# Project Event Consistency

`ProjectService` uses an application-level `IMilestoneEventHandler` boundary. It does not reference EF Core or call notification/audit repositories directly.

For create, status update, reopen, and delete operations, the service stages the Project mutation in the current unit of work, captures the previous and new state, and passes a tenant-scoped event to the handler. The handler stages the immutable audit entry and recipient notifications, then calls `IUnitOfWork.SaveChangesAsync` once.

This provides strict atomicity when the Project repository, audit repository, notification repository, and handler share the same EF Core `DbContext` and database transaction: the Project mutation, audit entry, and notifications commit together or roll back together.

Strict atomicity is not guaranteed if the handler is replaced by a remote service or external dispatcher. In that case, the Project Service must use a transactional outbox so the event is committed with the Project change and delivered asynchronously. The trade-off is eventual consistency: the Project write can commit before audit/notification consumers finish, but retries and idempotency keys prevent silent loss or duplicate records.

The current contract is milestone-shaped while the workspace currently has a Project aggregate and no Milestone aggregate. Until that domain model exists, Project lifecycle events use the documented event envelope and carry the Project identifier in the milestone slot so the existing team-recipient and audit pipeline can be exercised. This is an explicit compatibility assumption, not a claim that Projects and Milestones are the same domain entity.

The event handler must be registered with an `IMilestoneRecipientResolver` implementation before Project mutations are exposed. If it is not configured, `ProjectService` fails with `ProjectEventHandlerNotConfiguredException` rather than committing a Project mutation without its audit and notification side effects.