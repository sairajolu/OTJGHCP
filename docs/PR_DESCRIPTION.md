# TaskBridge Pull Request Description

## Summary

This change establishes the TaskBridge multi-tenant Project, Notification, and Audit foundations:

- Remediated Project Service layering and tenant context usage.
- Added Project lifecycle event integration for create, status update, reopen, and delete.
- Added immutable audit persistence and tenant-scoped audit queries.
- Added tenant- and recipient-scoped unread notifications.
- Added API endpoints for audit and notifications with authentication/policy attributes and OpenAPI wiring.
- Added trusted actor IP capture with nullable persistence and an additive EF migration.
- Added deterministic xUnit coverage for Project, Audit, Notification, tenant, lifecycle, privacy, and cancellation behavior.
- Added assessment documentation, impact analysis, consistency decisions, retention decision, review findings, and prompt evidence.

## Why the Change Was Needed

The initial Project model and service were retained unchanged as assessment evidence. Review identified missing tenant-aware application boundaries, status-transition validation, audit/notification integration, structured logging, and test coverage. The assessment also required milestone reopen events and later required actor IP capture with privacy controls.

## Project Service Remediation

`ProjectService` now:

- Uses `ICurrentUserContext` for authenticated `UserId` and `OrganisationId`.
- Passes `OrganisationId` to repository reads and mutations.
- Returns DTOs rather than domain or EF entities.
- Uses FluentValidation, `IClock`, async methods, and `CancellationToken`.
- Validates explicit status transitions, including `Completed -> Active` reopening.
- Emits application-level milestone events through `IMilestoneEventHandler`.
- Captures previous and new state snapshots.
- Uses structured logs without snapshots, secrets, or IP addresses.
- Fails closed if event handling is not configured.

## Notification and Audit Implementation

The domain and application layers include:

- `AuditEntry` with immutable fields, tenant identity, actor identity, event metadata, JSON snapshots, UTC timestamps, and nullable `ActorIpAddress`.
- `Notification` with recipient and organisation scope, event metadata, immutable content, and unread-to-read behavior.
- `MilestoneEventType` values for created, updated, deleted, and reopened events.
- Append-only audit repository contracts with no update/delete methods.
- Tenant- and recipient-scoped notification repositories.
- Idempotency lookup by event and recipient.
- Date, event-type, page-size, snapshot-size, identifier, enum, and UTC validation.
- DTO mappings that intentionally omit raw audit snapshots and actor IP from ordinary audit responses.

## Integration Contract

`MilestoneChangedEvent` is the application-level integration boundary. `MilestoneEventHandler`:

1. Validates tenant context.
2. Appends the audit entry.
3. Resolves eligible recipients through `IMilestoneRecipientResolver`.
4. Stages notifications.
5. Commits through the shared `IUnitOfWork`.

The current workspace has a Project aggregate but no Milestone aggregate, so Project lifecycle events use the milestone-shaped contract with the Project identifier in the milestone slot. This is documented as a compatibility assumption. A transactional outbox is the required direction when dispatch crosses process or service boundaries.

## AI Tool Disclosure

The implementation was produced primarily through GitHub Copilot Agent interactions using repository context. The session used workspace-scoped prompts, repository Copilot instructions, terminal validation, file inspection, and proposed-diff-first workflows.

### Specific Copilot Features Used

- Agent mode for multi-file implementation and review tasks.
- `@workspace` context for repository-wide architecture and security work.
- Repository instructions from `.github/copilot-instructions.md`.
- Chat-based architecture, security, privacy, and consistency analysis.
- Terminal-driven restore, build, migration, and test validation.
- Prompt logging in `docs/PROMPTS.md` for reproducibility.

Inline Chat, `/fix`, and Edit Mode were not used as separate interactions in the recorded session.

### Accepted Output

The following Copilot-assisted output was accepted after workspace verification:

- Layered solution structure and project references.
- Domain/application/infrastructure scaffolding.
- Project, Audit, and Notification models and repositories.
- Application services and event-handler abstractions.
- API controllers and OpenAPI wiring.
- Deterministic tests and documentation structure.

### Changed or Rejected Output

Human-directed review and follow-up corrections changed or rejected generated output in these cases:

- Retargeted SDK-generated projects to .NET 8 and converted `.slnx` to the requested classic solution format.
- Pinned EF Core packages to version 8.0.11.
- Corrected the generated migration from full schema creation to an additive nullable audit-column migration.
- Installed missing .NET 8 runtimes so tests could execute.
- Added a test event-handler double rather than weakening fail-closed production behavior.
- Corrected a staged-versus-committed audit test assertion.
- Corrected OpenAPI namespace and added the JWT bearer package required for compilation.
- Removed raw snapshots and IP from ordinary audit responses after the security review.
- Added the 90-day audit query bound and stable public validation errors.
- Did not implement unsupported project-membership authorization or invent a team-membership data store.

### Estimated AI Versus Hand-Written Code

Based on the recorded session, approximately 90% of implementation and documentation text was AI-generated or AI-assisted, and approximately 10% reflects human-directed design decisions, review constraints, manual corrections, and validation choices. No user-authored implementation code was observed in this session; this is an estimate, not a measured line count.

### Effect of `.github/copilot-instructions.md`

The repository instructions materially shaped the work. They enforced the controller -> application service -> repository -> EF Core direction, tenant filtering, authenticated context, DTO boundaries, async cancellation, FluentValidation, structured logging, append-only audit behavior, deterministic clocks, and security-focused tests. They also caused review attention to remain on cross-organisation access, sensitive logging, and audit immutability.

## Testing

Verified terminal results:

- `dotnet restore TaskBridge.sln`: passed.
- `dotnet build TaskBridge.sln`: passed.
- Affected test project: 22 passed, 0 failed, 0 skipped.
- Complete solution test suite: 22 passed, 0 failed, 0 skipped.

The suite uses deterministic GUIDs, a fake `IClock`, in-memory repository fakes, tenant contexts, event-handler fakes, and cancellation-token assertions.

## Known Gaps

- No Team or TeamMembership persistence model exists, so project/team membership authorization is not implemented.
- `GET /audit/{projectId}` is organisation-scoped but does not yet verify project membership or project-level permissions.
- The internal audit endpoint accepts relationship and snapshot fields from an authorized service request; trusted Project Service event-only writing and relationship validation remain future work.
- JWT issuer, audience, authority, and service-to-service validation values are not configured in the repository.
- The migration is present and pending; application startup still uses `EnsureCreatedAsync`, so deployment must apply the migration deliberately.
- Concurrent duplicate event delivery can still surface a database uniqueness error because the check-then-insert path is not fully atomic at the application boundary.
- Recipient resolution is an abstraction without a workspace-backed team-membership implementation.
- There are no API-host integration tests or production-database migration tests.
- The outbox is documented as the cross-service direction but is not implemented.

## Risks and Trade-offs

- Raw IP collection increases privacy obligations. The value is nullable, excluded from ordinary responses and logs, restricted by policy intent, and assigned a 90-day assessment retention decision. Production compliance approval remains required.
- Removing snapshots from ordinary audit responses is a contract change for any client that expected those fields, but prevents uncontrolled sensitive-state disclosure.
- The Project-to-milestone compatibility mapping allows the current assessment flow to run but must be replaced when a real Milestone aggregate exists.
- Shared unit-of-work atomicity applies only when Project, audit, and notification writes share the same database context. Remote dispatch requires an outbox and accepts eventual consistency.
- SQLite and `EnsureCreatedAsync` are assessment-friendly but are not a complete production migration/deployment strategy.

## Self-Review Checklist

- [x] Tenant identity is sourced from authenticated context, not request bodies.
- [x] Repository reads include organisation scope.
- [x] Notifications are restricted to the authenticated recipient and organisation.
- [x] Audit repositories expose no update/delete operations.
- [x] Raw audit snapshots and IP are excluded from ordinary audit responses.
- [x] Raw IP is not included in application log templates.
- [x] Forwarded headers are processed only for configured known proxies.
- [x] Audit date ordering, UTC values, page sizes, enum values, and snapshot sizes are validated.
- [x] Reopen events produce audit and notification processing.
- [x] Existing audit rows remain compatible with nullable IP storage.
- [x] Restore, build, and the complete test suite were run and passed.
- [ ] Project membership authorization is implemented.
- [ ] JWT issuer/audience/service validation is configured.
- [ ] Outbox delivery and migration deployment are implemented.

## Peer Review Simulation

1. **High, multi-tenant authorization:** In `AuditController.GetByProjectAsync` and `AuditService.GetByProjectAsync`, organisation filtering prevents cross-organisation reads but there is no project-membership or project-permission check. Add an organisation-scoped project access policy before returning audit history, with `404` for unauthorized or unavailable projects.

2. **High, integration integrity:** In `AuditController.PostAsync` and `AuditService.AppendAsync`, an authorized internal caller can supply `EntityId`, `ProjectId`, `MilestoneId`, and arbitrary valid JSON snapshots without a relationship check. Restrict writes to trusted Project Service events and validate that the referenced tenant-owned entities and snapshots match the event.

3. **Medium, operational consistency:** In `Program.cs` startup and `AddActorIpAddressToAuditEntries`, the application uses `EnsureCreatedAsync` while the additive migration remains pending. Define a baseline/migration deployment process before production rollout, or the new audit column can be absent from an existing database even though the EF model expects it.
