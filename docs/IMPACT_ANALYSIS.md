# Milestone Reopen and Actor IP Impact Analysis

## Change Summary

The requested scope change adds the `MILESTONE_REOPENED` event and requires audit entries to capture the actor's IP address.

In the verified workspace, the reopen behavior already exists internally as `MilestoneReopened` with enum value `4`. `ProjectService` emits it for the `Completed -> Active` transition, and `NotificationService` already creates a reopen notification title and message. The net-new implementation impact is therefore primarily actor IP capture, trusted request metadata handling, privacy controls, persistence schema, API/event contract decisions, and regression coverage.

No code was changed as part of this analysis.

## Affected Artifacts and Nature of Change

| Artifact | Impact | Nature |
|---|---|---|
| `src/TaskBridge.Domain/Enums/MilestoneEventType.cs` | Verify `MilestoneReopened = 4`; define external wire representation if uppercase `MILESTONE_REOPENED` is required. | Additive or contract-sensitive |
| `src/TaskBridge.Domain/Entities/AuditEntry.cs` | Add nullable `ActorIpAddress` with trusted-boundary validation. | Additive; model migration required |
| `src/TaskBridge.Application/Notifications/AuditService.cs` | Capture IP from trusted context or trusted event metadata; never from arbitrary request data. | Additive |
| `src/TaskBridge.Application/Notifications/NotificationService.cs` | Verify reopen notification behavior; current code already handles `MilestoneReopened`. | Verification, no functional change expected |
| `src/TaskBridge.Application/Notifications/NotificationContracts.cs` | Add IP only if the internal event envelope carries it; otherwise use a trusted request-context abstraction. | Internal contract change if event is expanded |
| `src/TaskBridge.Api/Controllers/AuditController.cs` | Do not trust client-supplied IP, actor, tenant, or timestamp; update OpenAPI only if the response exposes approved metadata. | Security-sensitive contract change |
| `src/TaskBridge.Application/Abstractions/ICurrentUserContext.cs` | May need a separate trusted request/network context rather than adding IP to the user identity abstraction. | Application contract decision |
| `src/TaskBridge.Infrastructure/Persistence/Configurations/AuditEntryConfiguration.cs` | Configure nullable text column with maximum length, recommended 45 characters. | Schema change |
| `src/TaskBridge.Infrastructure/Persistence/TaskBridgeDbContext.cs` | Include the new audit property through EF model configuration. | Schema change |
| Audit repository and query DTOs | Existing tenant-scoped queries continue to work; add IP filtering only if a justified security use case exists. | Usually no change |
| Database schema | Add nullable `ActorIpAddress`; preserve existing rows as `NULL`. | Migration required |
| Tests | Add reopen, IP capture, proxy trust, privacy, access-control, migration, and compatibility tests. | Additive |
| `docs/SPEC.md` | Document IP purpose, retention, access controls, and event wire naming. | Documentation change |
| `docs/PROJECT-EVENT-CONSISTENCY.md` | Document any event-envelope or outbox version change. | Documentation change |
| Logging configuration and runbooks | Ensure raw IP values are not written to logs or unrestricted diagnostics. | Security/privacy change |

## Migration Requirements

The current application uses `EnsureCreatedAsync`, and no migration-based schema path was verified. `EnsureCreatedAsync` is not a reliable way to evolve an existing audit table.

Recommended migration:

```text
ALTER audit table ADD ActorIpAddress TEXT NULL
```

Existing audit rows must remain readable with `NULL` because their original IP values cannot be reconstructed reliably. Do not backfill with a server address, placeholder address, or inferred value.

A production implementation should introduce and apply an EF Core migration before enabling writes that depend on the new column. The column should remain nullable until every writer and deployment path can supply trusted metadata.

## Compatibility Impact

### Event name

The code already uses `MilestoneReopened`. Renaming the C# member or serialized value could break clients, event consumers, filters, and persisted event representations.

Recommended compatibility approach:

- Retain enum value `4`.
- Retain the internal C# member unless a contract requires a change.
- Explicitly map the external wire value to `MILESTONE_REOPENED` if that exact spelling is required.
- Version event envelopes if IP metadata is added to a transported event.
- Support old and new envelopes during a transition period.

### Audit schema and API

Adding a nullable database column is backward-compatible for existing rows. Adding a required constructor parameter or required event field is potentially breaking for producers and test fixtures.

Raw IP should not be added to the ordinary audit response by default. If privileged audit access requires it, expose it through an explicitly authorized response or projection.

## Security and Privacy Risks

An IP address is sensitive network metadata and may constitute personal data or personal-data-adjacent information depending on jurisdiction and context. It must not be described as reliably identifying a person.

Risks include:

- Correlation of actor, organisation, project, and timestamp data.
- Exposure through audit APIs, exports, logs, backups, support tooling, or analytics.
- Retention beyond the documented business or compliance purpose.
- Inaccurate attribution when users share networks, devices, VPNs, or proxies.
- Cross-border storage or transfer obligations.
- Privilege expansion if ordinary users can view raw addresses.

Required controls:

- Capture only from a trusted request/network context.
- Never accept actor IP, actor identity, or `OrganisationId` as trusted body fields.
- Validate and normalize IPv4/IPv6 text, bounded to 45 characters.
- Store the field as nullable for historical compatibility.
- Keep raw IP out of ordinary structured logs.
- Restrict raw IP access to explicitly authorized audit/compliance users.
- Encrypt storage and backups according to the deployment environment.
- Define retention and deletion/archival controls outside the append-only audit API.
- Do not index the IP field unless a justified operational need exists.

### Proxy and forwarded-header trust

The workspace currently has no verified remote-IP capture or forwarded-header configuration. `X-Forwarded-For`, `Forwarded`, and similar headers must not be trusted directly from clients.

The implementation must configure trusted proxy networks, process forwarded headers only from trusted proxies, define the authoritative hop, and capture the resulting connection address. If trust cannot be established, store `NULL` rather than an unverified client value.

## Implementation Sequence

1. Confirm the external event spelling and serialization policy.
2. Obtain privacy/compliance approval for purpose, retention, access, storage region, and encryption.
3. Define a trusted request/network context abstraction.
4. Configure and test trusted proxy handling.
5. Add nullable `ActorIpAddress` to `AuditEntry`.
6. Add validation and normalization at the trusted boundary.
7. Update audit creation and internal event/outbox contracts, with versioning if transported externally.
8. Add an EF Core migration and deploy it before enabling new writes.
9. Keep IP out of ordinary audit responses unless privileged access is explicitly approved.
10. Update structured logging and operational documentation.
11. Add regression, privacy, proxy, migration, and compatibility tests.
12. Run restore, build, tests, migration checks, and security review.

## Validation and Testing Plan

### Event behavior

- `MilestoneReopened` creates one audit entry.
- `MilestoneReopened` notifies every eligible team member.
- Existing create, update, and delete event behavior remains unchanged.
- External filtering accepts the approved wire value.
- Unknown event values are rejected.

### IP capture and privacy

- Trusted IPv4 and IPv6 values are stored correctly.
- Missing IP is stored as `NULL` when policy permits.
- Invalid values are rejected or converted to `NULL` according to the approved policy.
- Client-supplied IP cannot override trusted context.
- Raw IP is absent from ordinary logs, notifications, and non-privileged responses.
- Authorized audit access is tested separately if raw IP exposure is approved.

### Proxy and tenant security

- Trusted forwarded headers resolve the configured client address.
- Untrusted forwarded headers are ignored.
- Multiple proxy hops follow the trust policy.
- Audit queries remain organisation-scoped.
- Cross-organisation requests do not disclose resource existence.

### Persistence and compatibility

- Migration adds a nullable column without destroying existing rows.
- Existing rows with `NULL` IP remain queryable.
- Event value `4` remains compatible.
- Duplicate-event idempotency continues to work.
- Rollback-compatible event envelopes are accepted where required.

## Rollback Considerations

- Deploy the nullable schema column before code that writes it.
- A rollback to code that does not read the column remains compatible because the column is additive and nullable.
- Do not remove the column during rollback if newer rows contain IP data.
- If privacy approval is withdrawn, disable new collection through a controlled configuration path and retain existing data according to the approved retention decision.
- Keep old and new event envelope versions during a consumer rollback window.
- Preserve existing audit rows and their `NULL` values.

## How Copilot Assisted This Analysis

**Prompt used:**

> Act as a senior solution architect and privacy-focused security reviewer.
>
> A scope change has been received:
>
> "Add a new milestone event type: MILESTONE_REOPENED. This should trigger audit logging and notifications. Audit entries must now also capture the actor's IP address."
>
> Before modifying code, analyse the impact.
>
> List:
> - every affected file, module, model, enum, DTO, database configuration, migration, service, endpoint, validator, test, API contract, document, and logging concern,
> - whether each change is additive, breaking, or needs migration,
> - backward-compatibility impact,
> - handling of existing audit rows,
> - privacy and compliance risk of storing IP addresses,
> - retention, masking, encryption, access-control, and logging-exposure concerns,
> - proxy and forwarded-header trust risks,
> - recommended implementation order,
> - rollback considerations,
> - additional tests.
>
> Do not change any code. Do not state that an IP address reliably identifies a person.

Copilot identified that `MilestoneReopened` already exists as enum value `4`, that the Project Service and Notification Service already contain reopen behavior, and that the net-new work is primarily IP capture, persistence schema, trusted request metadata, privacy controls, API/event compatibility, and testing. It also identified that the current workspace uses `EnsureCreatedAsync`, that existing audit rows cannot be reliably backfilled, and that forwarded headers require an explicit trusted-proxy policy.

Human validation was required to confirm the workspace references, distinguish already-implemented reopen behavior from genuinely new work, decide that existing IP values cannot be reconstructed, reject blind trust of forwarded headers, avoid treating an IP address as a reliable person identifier, and determine that raw IP should not be exposed through ordinary audit responses. No code correction or implementation was performed for this scope-change analysis.
