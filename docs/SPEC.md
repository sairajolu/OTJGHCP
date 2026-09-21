# TaskBridge Notification and Audit Service Specification

## Purpose and Scope

The service records immutable, organisation-scoped audit history and creates in-app notifications for relevant team members when project milestones are created, updated, deleted, or reopened.

It supports audit creation/querying, notification retrieval/read state, tenant isolation, authorization, validation, and reliable Project Service integration. Email, SMS, push delivery, user administration, and audit deletion are out of scope.

## Domain Models

### AuditEntry

| Field | Type | Notes |
|---|---|---|
| `Id` | `Guid` | Server-generated |
| `EventId` | `Guid` | Idempotency key |
| `OrganisationId` | `Guid` | Required tenant boundary |
| `ActorUserId` | `Guid` | Authenticated actor |
| `ProjectId` | `Guid` | Required |
| `MilestoneId` | `Guid?` | Required for milestone events |
| `EventType` | `AuditEventType` | Required enum |
| `OccurredAtUtc` | `DateTime` | UTC, server-controlled |
| `BeforeStateJson` | `string?` | Approved, redacted fields only |
| `AfterStateJson` | `string?` | Approved, redacted fields only |
| `CorrelationId` | `string` | Bounded tracing identifier |
| `CreatedAtUtc` | `DateTime` | UTC, server-controlled |

### Notification

| Field | Type | Notes |
|---|---|---|
| `Id` | `Guid` | Server-generated |
| `EventId` | `Guid` | Idempotency key |
| `OrganisationId` | `Guid` | Required tenant boundary |
| `RecipientUserId` | `Guid` | Required recipient |
| `ActorUserId` | `Guid` | Event initiator |
| `ProjectId` | `Guid` | Required |
| `MilestoneId` | `Guid?` | Required for milestone events |
| `EventType` | `NotificationEventType` | Required enum |
| `Title` | `string` | Required, length-limited |
| `Message` | `string` | Required, length-limited |
| `CreatedAtUtc` | `DateTime` | UTC, server-controlled |
| `ReadAtUtc` | `DateTime?` | Set when read |

Supported event types are `MilestoneCreated`, `MilestoneUpdated`, `MilestoneDeleted`, and `MilestoneReopened`.

## Contracts and Endpoints

Requests must not accept trusted `OrganisationId`, actor identity, audit IDs, or server timestamps. Responses expose DTOs, not persistence entities.

- `POST /audit` — authorized Project Service/service identity appends an audit entry; returns `201 Created`.
- `GET /audit/{projectId}?from=&to=&eventType=` — returns tenant-scoped, paginated audit entries.
- `GET /notifications/{userId}` — returns notifications for the authenticated user and organisation.
- `PATCH /notifications/{id}/read` — marks the authenticated recipient’s notification as read; the operation is idempotent.

All list endpoints require deterministic ordering and bounded pagination. Date filters must be UTC and `from <= to`; event filters must contain defined enum values.

## Integration with Project Service

The Project Service emits an event envelope containing `EventId`, `OrganisationId`, `ActorUserId`, `ProjectId`, `MilestoneId`, `EventType`, `OccurredAtUtc`, `CorrelationId`, and `SchemaVersion`.

The preferred flow is a transactional outbox:

```text
Project Service -> milestone change + audit/outbox records -> commit
               -> outbox dispatcher -> Notification/Audit processing
```

Consumers must be idempotent. `EventId` and recipient identity prevent duplicate audit records and notifications.

Recipients are active members of the milestone’s team in the same organisation. The initiating actor is excluded by default; this is an engineering decision and may be changed by product policy.

## Security and Tenant Isolation

**Assessment requirements:**

- Every tenant-owned record contains `OrganisationId`.
- Organisation and actor identity come from authenticated context, never request bodies.
- Audit records are append-only.
- Tenant-scoped reads, writes, and notification access are mandatory.

**Engineering decisions:**

- Cross-tenant or unavailable resources return `404` without confirming ownership.
- Notification access requires both `OrganisationId` and authenticated `RecipientUserId`.
- Audit writing is restricted to the Project Service or an authorized service identity.
- Snapshot fields are allowlisted and redacted before persistence.
- The initiating actor is excluded from notifications by default.

Authentication must validate issuer, audience, signature, and required claims. Authorization should distinguish `Audit.Read`, internal audit writing, notification reading, and marking notifications read.

## Validation and Immutability

Validate non-empty identifiers, enum values, bounded titles/messages/correlation IDs, valid project/milestone relationships, UTC date filters, maximum date ranges, page sizes, and snapshot size. Client-provided tenant identity, actor identity, occurrence times, and audit IDs are rejected or ignored.

Audit entries have no update or delete endpoint or repository operation. EF Core configuration and database permissions should prevent accidental modification where supported. Notifications may change only from unread to read; content and ownership are immutable.

## Errors and Consistency

Use `ProblemDetails` responses:

- `400` invalid input or filters
- `401` unauthenticated
- `403` unauthorized
- `404` unavailable or cross-tenant resource
- `409` duplicate event or concurrency conflict
- `500` unexpected persistence failure without internal details

Audit, outbox, and milestone changes should commit in one transaction. Remote notification delivery must occur after commit through the outbox dispatcher, not inside the database transaction. UTC values come from an injectable `IClock` and are persisted and queried consistently as UTC.

## Test Acceptance Criteria

Tests must verify:

- all four milestone event types;
- append-only audit behavior;
- duplicate-event idempotency;
- date and event-type filtering;
- bounded pagination;
- tenant and recipient isolation;
- authorization and invalid input;
- notification recipient determination and read idempotency;
- UTC timestamps;
- transaction rollback and retry behavior;
- SQLite-backed repository behavior.

## Copilot Assistance and Human Judgment

Copilot helped draft the structure, model fields, endpoint contracts, validation rules, outbox flow, error mapping, and test acceptance criteria from the assessment requirements and repository instructions.

Human validation was required for security-sensitive decisions: treating authentication context as the only trusted source of organisation and actor identity; avoiding cross-tenant resource disclosure; restricting audit writes; selecting an allowlist for snapshots; excluding the initiating actor from notifications; and requiring idempotent, transactional event processing. These decisions affect privacy, tenant isolation, compliance evidence, and downstream consistency and must not be accepted solely because generated text is plausible.

This specification records design decisions from the planning discussion. It does not claim that implementation corrections or production validation have occurred.
