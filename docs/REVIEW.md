# Initial Project Code Review

This review covers the preserved files in `docs/initial-generated-code` and the supporting implementation that the original Project model and service depend on. Findings are limited to behavior confirmed by the workspace code.

## Confirmed Issues

### 1. Authentication is not configured

- **Number:** 1
- **Exact file and function:** `src/TaskBridge.Api/Program.cs`, application startup between `builder.Build()` and `app.Run()`
- **Category:** Authentication and authorization
- **Severity:** Critical
- **Issue description:** The application calls `UseAuthorization()` but does not configure an authentication scheme, call `UseAuthentication()`, or require an authenticated policy for the Project operations.
- **Multi-tenant B2B SaaS impact:** The application has no verified request identity from which to establish the actor or organisation. A tenant boundary based on claims cannot be trusted when authentication is not enforced.
- **Detection method:** Ask Mode review
- **Recommended fix:** Configure a trusted JWT/OIDC authentication scheme, add authentication middleware before authorization middleware, and require authenticated authorization policies on Project endpoints.

### 2. Project operations do not enforce actor authorization

- **Number:** 2
- **Exact file and function:** `docs/initial-generated-code/ProjectService.cs`, `CreateAsync`, `UpdateStatusAsync`, `GetByTeamAsync`, and `DeleteAsync`
- **Category:** Authentication and authorization
- **Severity:** High
- **Issue description:** The service receives `ICurrentTenant`, including `UserId`, but never uses the actor identity to check permissions, organisation membership, role, or team membership before reading or mutating a project.
- **Multi-tenant B2B SaaS impact:** A caller who reaches the service could perform project operations without the operation-specific permissions expected in a B2B organisation.
- **Detection method:** Ask Mode review
- **Recommended fix:** Add an application authorization abstraction and enforce operation-specific permissions and membership checks before each read or mutation.

### 3. Project creation does not validate team ownership

- **Number:** 3
- **Exact file and function:** `docs/initial-generated-code/ProjectService.cs`, `CreateAsync`
- **Category:** Multi-tenant data isolation
- **Severity:** High
- **Issue description:** `request.TeamId` is copied into the new Project without checking that the team exists, belongs to `currentTenant.OrganisationId`, or is accessible to the actor.
- **Multi-tenant B2B SaaS impact:** A project can contain an invalid or cross-organisation team relationship. Downstream team queries, notifications, and authorization decisions could then use an unsafe relationship.
- **Detection method:** Ask Mode review
- **Recommended fix:** Perform an organisation-scoped team and membership lookup before creating the project, and enforce the relationship at the persistence boundary where possible.

### 4. Project status transitions are unrestricted

- **Number:** 4
- **Exact file and function:** `docs/initial-generated-code/Project.cs`, `UpdateStatus`; `docs/initial-generated-code/ProjectService.cs`, `UpdateStatusAsync`
- **Category:** Correctness and domain rules
- **Severity:** Medium
- **Issue description:** `UpdateStatus` assigns any `ProjectStatus` value without checking the current status or allowed transitions. The service validates that the enum value is defined, but not that the transition is valid.
- **Multi-tenant B2B SaaS impact:** Invalid lifecycle states can reach reporting, workflow, notification, and integration services and produce inconsistent tenant project data.
- **Detection method:** Ask Mode review
- **Recommended fix:** Define and enforce allowed status transitions in the domain model, returning a specific domain or application error for invalid transitions.

### 5. Domain factory and mutation methods do not enforce core invariants

- **Number:** 5
- **Exact file and function:** `docs/initial-generated-code/Project.cs`, `Create` and `UpdateStatus`
- **Category:** Input validation and correctness
- **Severity:** Medium
- **Issue description:** `Create` accepts empty organisation and team identifiers, blank names, and non-UTC timestamps. `UpdateStatus` accepts any enum-castable status and any timestamp kind. These invariants are not enforced inside the domain methods.
- **Multi-tenant B2B SaaS impact:** Other callers such as background jobs, future integrations, or additional application services can create invalid tenant-owned records while bypassing the current service validators.
- **Detection method:** manual review
- **Recommended fix:** Enforce domain invariants in `Create` and `UpdateStatus`, including non-empty identifiers, non-blank normalized names, valid status values, and UTC timestamps.

### 6. Whitespace-only names can be persisted as empty names

- **Number:** 6
- **Exact file and function:** `src/TaskBridge.Application/Projects/ProjectValidators.cs`, `CreateProjectRequestValidator`; `docs/initial-generated-code/ProjectService.cs`, `CreateAsync`
- **Category:** Input validation
- **Severity:** Medium
- **Issue description:** `NotEmpty()` does not reject a whitespace-only name. `CreateAsync` then calls `Trim()` and can pass an empty string to `Project.Create`.
- **Multi-tenant B2B SaaS impact:** Tenant workspaces can contain projects with unusable or indistinguishable names, affecting search, reporting, and downstream synchronization.
- **Detection method:** Ask Mode review
- **Recommended fix:** Validate the trimmed value as non-whitespace at the application boundary and normalize only after validation succeeds.

### 7. Project status updates have no optimistic concurrency protection

- **Number:** 7
- **Exact file and function:** `docs/initial-generated-code/ProjectService.cs`, `UpdateStatusAsync`; `src/TaskBridge.Infrastructure/Persistence/TaskBridgeDbContext.cs`, `OnModelCreating`
- **Category:** Concurrency
- **Severity:** High
- **Issue description:** The Project has no configured version or concurrency token. Concurrent status updates can therefore allow the later save to overwrite the earlier save without a conflict.
- **Multi-tenant B2B SaaS impact:** Users or services operating in the same organisation can lose updates silently, causing inaccurate project state and inconsistent downstream events.
- **Detection method:** Ask Mode review
- **Recommended fix:** Add an EF Core concurrency token or version value, require the expected version in update requests, and translate concurrency failures into a conflict response.

### 8. Project mutations do not create audit records or notifications

- **Number:** 8
- **Exact file and function:** `docs/initial-generated-code/ProjectService.cs`, `CreateAsync`, `UpdateStatusAsync`, and `DeleteAsync`
- **Category:** Transactional consistency and architecture
- **Severity:** High
- **Issue description:** The service has no audit or notification dependency and performs no audit append or notification creation for project mutations.
- **Multi-tenant B2B SaaS impact:** Required tenant activity history is absent, and relevant team members receive no operational signal. Downstream audit, compliance, and notification consumers have nothing to process.
- **Detection method:** Ask Mode review
- **Recommended fix:** Integrate explicit audit and notification abstractions. Capture actor, organisation, action, and before/after state, and commit related records transactionally or through a transactional outbox.

### 9. Team project reads are unbounded

- **Number:** 9
- **Exact file and function:** `docs/initial-generated-code/ProjectService.cs`, `GetByTeamAsync`; `src/TaskBridge.Infrastructure/Repositories/ProjectRepository.cs`, `GetByTeamAsync`
- **Category:** Performance and API contract safety
- **Severity:** Medium
- **Issue description:** The repository loads every matching project with `ToListAsync`, and the service materializes another array with `ToArray()`. There is no page size, limit, continuation token, or other bound.
- **Multi-tenant B2B SaaS impact:** A large tenant or team can cause excessive memory use and slow responses, creating resource contention for other tenants sharing the service.
- **Detection method:** Ask Mode review
- **Recommended fix:** Add validated pagination and deterministic ordering, and project directly to response DTOs where appropriate.

### 10. Tenant and security behavior is not tested against the real database

- **Number:** 10
- **Exact file and function:** `tests/TaskBridge.Tests/ProjectServiceTests.cs`, `ProjectServiceTests` and `InMemoryProjectRepository`
- **Category:** Testing and multi-tenant data isolation
- **Severity:** Medium
- **Issue description:** The tests use an in-memory repository fake rather than the EF Core SQLite repository. They cover successful paths but do not test cross-organisation reads, updates, deletes, team ownership, authorization, invalid transitions, concurrency, audit behavior, or notification behavior.
- **Multi-tenant B2B SaaS impact:** A regression in EF mapping, repository filtering, or database behavior could expose tenant data while the existing tests continue to pass.
- **Detection method:** Ask Mode review
- **Recommended fix:** Add SQLite integration tests for tenant-scoped CRUD and add unit/integration tests for validation, not-found behavior, authorization, concurrency, audit immutability, and notification dispatch.

## Architectural & Security Issues Copilot Introduced That Required Human Judgment

### Missing enforcement behind tenant context

Copilot generated a service that passes `OrganisationId` into repository queries, which appears tenant-aware, but it did not enforce authentication, actor authorization, or team ownership. Human domain knowledge was required to recognize that a tenant ID parameter is not sufficient proof that the caller is entitled to use it. In a B2B SaaS platform, downstream authorization, notification, reporting, and integration services may trust the project-to-team relationship and actor metadata. An unauthorized or cross-organisation relationship can therefore propagate beyond the original request.

### Missing audit and notification workflows

Copilot generated the core Project CRUD/status flow but omitted the required audit and notification integration entirely. Human review was needed to compare the implementation with the platform’s operational and compliance requirements rather than treating successful database writes as a complete feature. Downstream audit consumers may rely on append-only change records, while team-facing services may rely on notifications to trigger work or synchronization. Silent omissions can create compliance gaps, stale user views, and inconsistent distributed workflows.

### Missing lifecycle and concurrency rules

Copilot accepted every defined status as a valid transition and provided no optimistic concurrency token. Human product and architecture judgment was required to identify that enum validation does not establish a valid state machine and that a last-write-wins update is unsafe for shared B2B project data. Downstream reporting and workflow services can act on invalid or silently overwritten states, making the defect difficult to reconcile after multiple clients update the same project.
