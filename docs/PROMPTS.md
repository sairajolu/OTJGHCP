# TaskBridge Prompt Log

<table>
  <thead>
    <tr>
      <th>Prompt Number</th>
      <th>Exact Prompt</th>
      <th>Copilot Mode or Feature</th>
      <th>Context Used</th>
      <th>Prompting Technique</th>
      <th>Why This Approach</th>
      <th>Result</th>
      <th>Post-Generation Correction</th>
    </tr>
  </thead>
  <tbody>
    <tr>
      <td>1</td>
      <td><pre>Act as a senior .NET solution architect helping me plan a GitHub Copilot practitioner assessment.

The application is TaskBridge, a multi-tenant B2B SaaS platform. I will use .NET 8, ASP.NET Core Web API, Entity Framework Core, SQLite, xUnit, FluentValidation, and Serilog.

The solution must contain:
1. A Project Service.
2. A Notification and Audit Service.
3. Layered architecture: controller -> service -> repository -> EF Core model.
4. Organisation-based tenant isolation.
5. Immutable audit entries.
6. Notification creation for relevant team members when a project milestone is created, updated, deleted, or reopened.
7. Technical documentation and tests.

For now, do not generate implementation code. Propose:
- solution and folder structure,
- project references,
- domain boundaries,
- main interfaces,
- data flow,
- security boundaries,
- likely technical risks,
- implementation sequence.

Clearly distinguish required assessment work from optional improvements.</pre></td>
      <td>Chat / architecture planning</td>
      <td>TaskBridge requirements and selected .NET technology stack.</td>
      <td>Role framing, explicit constraints, and structured deliverables.</td>
      <td>To establish boundaries and sequencing before implementation.</td>
      <td>Architecture proposal was produced in the chat response and is verifiable in the session.</td>
      <td>None required.</td>
    </tr>
    <tr>
      <td>2</td>
      <td><pre>Create the TaskBridge .NET 8 solution structure based on the agreed plan.

Create:
- src/TaskBridge.Api
- src/TaskBridge.Application
- src/TaskBridge.Domain
- src/TaskBridge.Infrastructure
- tests/TaskBridge.Tests
- docs
- .github

Create TaskBridge.sln and add all projects to it.

Apply these references:
- Api references Application and Infrastructure
- Application references Domain
- Infrastructure references Application and Domain
- Tests reference all application projects required for unit and integration testing

Add the required NuGet packages for ASP.NET Core Web API, EF Core SQLite, EF Core Design, xUnit, FluentAssertions, FluentValidation, Serilog, and OpenAPI.

Do not implement domain features yet. Show every file created and every command proposed before applying changes.</pre></td>
      <td>Agent / workspace scaffolding</td>
      <td>Previously agreed architecture plan and the existing TaskBridge workspace.</td>
      <td>Explicit file, reference, package, and scope checklist.</td>
      <td>To create a verifiable foundation while preventing premature domain implementation.</td>
      <td>Solution and projects were created; references and packages were verified in the workspace. Build passed.</td>
      <td>Adjusted for SDK behavior: generated with the installed SDK, converted to classic `.sln`, retargeted to `net8.0`, and pinned EF Core to `8.0.11`. Tests were blocked by the missing .NET 8 runtime.</td>
    </tr>
    <tr>
      <td>3</td>
      <td><pre>Create .github/copilot-instructions.md for this TaskBridge repository.

The instructions must tell GitHub Copilot to follow these standards:

Technology:
- .NET 8 and C#
- ASP.NET Core Web API
- Entity Framework Core with SQLite
- xUnit and FluentAssertions
- FluentValidation
- Serilog structured logging

Architecture:
- Controller -> application service -> repository -> EF Core model
- Controllers contain HTTP concerns only
- Services contain business logic only
- Repositories contain persistence logic only
- No raw SQL or database-driver calls in services
- Use asynchronous methods and CancellationToken
- Use dependency injection and interfaces
- Do not expose EF entities as API responses

Coding:
- Nullable reference types enabled
- Explicit request and response DTOs
- PascalCase for public members and camelCase for local variables
- Small focused methods
- XML documentation on public types and methods
- Specific exceptions rather than generic Exception
- No empty catch blocks

Multi-tenancy and security:
- Every tenant-owned entity includes OrganisationId
- Obtain actor and OrganisationId from an authenticated request context, never trust client-supplied tenant identity
- Every tenant-owned repository query must filter by OrganisationId
- Prevent cross-organisation access
- Validate identifiers, enum values, date ranges, and input lengths
- Never log secrets, tokens, complete request bodies, snapshots containing sensitive data, or raw IP addresses
- Use structured logging with named properties
- No hardcoded credentials or PII

Audit:
- Audit records are append-only
- Do not expose update or delete operations for audit entries
- Record before and after state where applicable
- Prevent accidental EF Core modification of existing audit records

Testing:
- Cover happy paths, validation, not-found conditions, tenant isolation, authorization, audit immutability, date filtering, event filtering, and notification dispatch
- Use deterministic UTC timestamps through an injectable clock abstraction

When any requirement is ambiguous, prefer the safest multi-tenant behavior and document the assumption.</pre></td>
      <td>Agent / repository customization</td>
      <td>Current `.csproj` files and the TaskBridge architecture and security requirements.</td>
      <td>Structured standards grouped by concern.</td>
      <td>To make the agreed engineering constraints persist across future Copilot work.</td>
      <td>[`.github/copilot-instructions.md`](../.github/copilot-instructions.md) was created and verified.</td>
      <td>None required.</td>
    </tr>
    <tr>
      <td>4</td>
      <td><pre>Create docs/PROMPTS.md.

Add a heading and a table with these columns:
- Prompt Number
- Exact Prompt
- Copilot Mode or Feature
- Context Used
- Prompting Technique
- Why This Approach
- Result
- Post-Generation Correction

Populate entries for the prompts already used in this session without changing their wording. Continue appending every later implementation, review, testing, and documentation prompt to this file.

Do not claim a prompt succeeded unless its resulting files or response can be verified in the workspace.</pre></td>
      <td>Agent / documentation</td>
      <td>Prompts and verified outcomes from this session.</td>
      <td>Requirements-based documentation with an exact-prompt audit trail.</td>
      <td>To preserve reproducibility and distinguish verified outcomes from assumptions.</td>
      <td>[`docs/PROMPTS.md`](PROMPTS.md) was created with all prompts used so far and verified in the workspace.</td>
      <td>None required.</td>
    </tr>
    <tr>
      <td>5</td>
      <td><pre>Generate a Project model and a Project service with create, update status, get by team, and delete functions. Use a database.</pre></td>
      <td>Agent / implementation</td>
      <td>Existing TaskBridge solution structure, repository Copilot instructions, and the agreed layered multi-tenant architecture.</td>
      <td>Small vertical slice with explicit domain, application, persistence, and test boundaries.</td>
      <td>To implement the requested behavior while preserving tenant isolation and the controller-service-repository- database flow.</td>
      <td>Project model, service, SQLite DbContext, tenant-filtered repository, DI registration, startup database initialization, and focused tests were created. The solution build passed.</td>
      <td>EF CLI was unavailable, so the initial scaffold uses `EnsureCreatedAsync` instead of a generated migration. Test execution remains blocked by the missing .NET 8 runtime.</td>
    </tr>
    <tr>
      <td>6</td>
      <td><pre>Preserve the current unreviewed AI-generated Project model and Project service as assessment evidence.

Do not modify their content.

Create docs/initial-generated-code and copy the exact current files into that folder. Add a short README stating that these files are the untouched output from the mandatory low-effort prompt and are retained only for comparison and review evidence.</pre></td>
      <td>Agent / documentation and evidence preservation</td>
      <td>Current Project model, Project service, supporting ProjectStatus enum, and existing prompt log.</td>
      <td>Evidence snapshot with byte-level verification.</td>
      <td>To preserve the unreviewed generated implementation for assessment comparison without altering source content.</td>
      <td>The evidence folder, exact file copies, and README were created and source/copy SHA-256 hashes matched.</td>
      <td>Included the supporting `ProjectStatus.cs` snapshot because the model and service reference it; source files were not modified.</td>
    </tr>
    <tr>
      <td>7</td>
      <td><pre>Act as a senior application security engineer and .NET architect.

Review the unmodified Project model and Project service currently in the workspace. Do not fix anything yet.

Identify concrete issues under:
- correctness,
- multi-tenant data isolation,
- authentication and authorization,
- injection or unsafe data access,
- mass assignment,
- input validation,
- error handling,
- concurrency,
- transactional consistency,
- architecture and separation of concerns,
- asynchronous programming,
- performance,
- logging and sensitive-data exposure,
- maintainability,
- testing,
- API contract safety.

For each issue provide:
1. Exact file and method.
2. Category.
3. Severity: Critical, High, Medium, or Low.
4. What is wrong.
5. Multi-tenant or B2B SaaS impact.
6. Recommended fix.

Only report issues supported by the actual code. Do not invent missing behavior.</pre></td>
      <td>Agent / security and architecture review</td>
      <td>Unmodified Project model and service, their validators and repository, EF model, tenant context, API pipeline, and existing tests.</td>
      <td>Evidence-based risk review grouped by severity and requested category.</td>
      <td>To identify concrete security, correctness, and operational risks before remediation.</td>
      <td>Review findings were produced from the workspace code without modifying the Project model or service.</td>
      <td>None; source implementation was intentionally left unchanged.</td>
    </tr>
    <tr>
      <td>8</td>
      <td><pre>Using only the confirmed issues from our review of the preserved initial Project files, create docs/REVIEW.md.

For every issue include:
- number,
- exact file and function,
- category,
- severity,
- issue description,
- multi-tenant B2B SaaS impact,
- detection method,
- recommended fix.

The detection method must accurately state one of:
- Ask Mode review,
- @workspace comparison,
- /explain,
- Inline Chat,
- manual review.

Include at least eight issues only if eight are supported by the code. Do not fabricate issues to reach a count.

Finish with:
"Architectural &amp; Security Issues Copilot Introduced That Required Human Judgment"

In that section, discuss at least two real issues that Copilot created or failed to identify, why human domain knowledge was needed, and why each is risky for downstream services.</pre></td>
      <td>Agent / security review documentation</td>
      <td>Preserved initial Project files, supporting validators, repository, EF Core context, API startup, tests, and confirmed findings from the prior review.</td>
      <td>Evidence-based issue register with detection provenance and human-judgment analysis.</td>
      <td>To document only verified deviations and explain risks that require architectural and domain review.</td>
      <td><code>docs/REVIEW.md</code> was created with ten confirmed issues and a final human-judgment section. The file was written from workspace evidence.</td>
      <td>None required; no implementation files were modified.</td>
    </tr>
    <tr>
      <td>9</td>
      <td><pre>@workspace Review this remediated service against .github/copilot-instructions.md.

Make only targeted changes needed to ensure:
- business logic is independent of EF Core,
- tenant identity comes from ICurrentUserContext,
- all repository operations are tenant-scoped,
- status transitions are validated,
- CancellationToken is propagated,
- exceptions are specific,
- logging is structured and safe.

Show a diff and explain each change. Do not refactor unrelated files.</pre></td>
      <td>Agent / targeted code review and remediation</td>
      <td>Current ProjectService, project repository contract, current tenant abstraction, API registration, and repository Copilot instructions.</td>
      <td>Focused issue-to-fix remediation with minimal file scope.</td>
      <td>To remediate only confirmed service-level deviations while preserving the existing architecture.</td>
      <td>ProjectService now uses ICurrentUserContext, validates status transitions, propagates cancellation, throws a specific transition exception, and emits safe structured logs. The solution build passed.</td>
      <td>Added a compatibility relationship from ICurrentTenant to ICurrentUserContext and retained the existing HTTP implementation name to avoid unrelated test refactoring.</td>
    </tr>
    <tr>
      <td>10</td>
      <td><pre>Create docs/SPEC.md from the agreed specification.

Keep it concise enough for approximately one to two pages. Add a section titled "Copilot Assistance and Human Judgment" identifying which parts Copilot helped draft and which security, multi-tenancy, privacy, or consistency decisions required human validation.

Do not claim human corrections that did not occur in this session.</pre></td>
      <td>Agent / technical documentation</td>
      <td>Agreed Notification and Audit Service specification, assessment requirements, remediated Project Service design, and repository Copilot instructions.</td>
      <td>Concise specification synthesis with explicit provenance and human-validation boundaries.</td>
      <td>To produce an assessment-ready technical specification without overstating implementation or review outcomes.</td>
      <td><code>docs/SPEC.md</code> was created with the requested scope, contracts, security, consistency, validation, testing, and human-judgment sections.</td>
      <td>None; the document explicitly states that implementation corrections and production validation have not occurred.</td>
    </tr>
    <tr>
      <td>11</td>
      <td><pre>@workspace Implement the Notification and Audit application services according to docs/SPEC.md.

Required behavior:
- On project milestone create, update-status, delete, and reopen events, append one immutable audit entry.
- Create one notification for every relevant team member resolved through an abstraction.
- Capture previous and new state.
- Scope all operations by the authenticated OrganisationId.
- Permit audit queries by project ID with optional from, to, and eventType filters.
- Return unread notifications only for the requested user.
- A user may retrieve or mark only their own notifications unless an explicitly authorized administrative policy exists.
- Validate from &lt;= to.
- Use IClock for timestamps.
- Use structured logging.
- Use async and CancellationToken.
- Do not add update or delete operations for audit records.
- Do not expose EF Core entities directly.

Add interfaces for recipient resolution and milestone-event handling so external dispatch mechanisms can later replace the in-process implementation.

Do not create controllers or tests in this step.</pre></td>
      <td>Agent / application and persistence implementation</td>
      <td><code>docs/SPEC.md</code>, existing Project Service patterns, EF Core DbContext, repository conventions, current-user context, and Copilot instructions.</td>
      <td>Layered vertical slice with append-only persistence, tenant-scoped queries, idempotent event handling, and explicit extension abstractions.</td>
      <td>To implement audit and notification application behavior without adding controllers or tests.</td>
      <td>Domain models, EF configurations, repositories, application services, event-handler and recipient-resolver interfaces, unit-of-work integration, and DI registrations were created. The solution build passed.</td>
      <td>No controllers or tests were added. Recipient resolution remains an injected abstraction because team-membership persistence is not present in the workspace.</td>
    </tr>
    <tr>
      <td>12</td>
      <td><pre>@workspace Implement the API endpoints defined in docs/SPEC.md:

1. POST /audit
2. GET /audit/{projectId}?from=&amp;to=&amp;eventType=
3. GET /notifications/{userId}
4. PATCH /notifications/{id}/read

Requirements:
- Controllers contain no business or persistence logic.
- Use typed request and response DTOs.
- Apply validation.
- Use authenticated UserId and OrganisationId from ICurrentUserContext.
- POST /audit must be protected as an internal service endpoint and must not trust actor or tenant details supplied by arbitrary callers.
- Cross-tenant resource access must return a non-disclosing response.
- Use specific exception-to-HTTP mappings.
- Propagate CancellationToken.
- Add OpenAPI documentation.
- Do not expose update or delete endpoints for audit entries.
- Show the proposed changes first.</pre></td>
      <td>Agent / API implementation</td>
      <td><code>docs/SPEC.md</code>, existing application service contracts, current-user context, API composition root, and repository Copilot instructions.</td>
      <td>Thin controllers with typed binding, policy protection, non-disclosing responses, and cancellation propagation.</td>
      <td>To expose only the specified audit and notification operations while preserving application-layer ownership of business logic.</td>
      <td>Audit and notification controllers, OpenAPI documentation wiring, JWT authentication middleware, and the internal audit policy were added. The solution build passed.</td>
      <td>The OpenAPI namespace and JWT bearer package were adjusted to match the installed .NET 8-compatible package versions.</td>
    </tr>
    <tr>
      <td>13</td>
      <td><pre>@workspace Integrate milestone changes in ProjectService with the Notification and Audit feature.

For project creation, status update, deletion, and reopen:
- load and validate the tenant-owned project,
- capture the previous state before mutation,
- apply business validation,
- persist the project change,
- record the audit event,
- create notifications for all relevant team members.

Use the integration contract documented in docs/SPEC.md.

Do not create circular project references. Keep ProjectService free of EF Core dependencies. Use an application-level interface or domain event abstraction.

Explain and document the selected consistency strategy. If strict atomicity cannot be guaranteed across service boundaries, state that clearly and identify the trade-off instead of hiding it.</pre></td>
      <td>Agent / application integration</td>
      <td>Remediated ProjectService, Notification and Audit services, integration contract in <code>docs/SPEC.md</code>, EF unit-of-work abstraction, and current tenant context.</td>
      <td>Application-level event integration with staged mutations, immutable side effects, and explicit consistency assumptions.</td>
      <td>To connect Project lifecycle changes to audit and notification processing without introducing EF dependencies or circular references.</td>
      <td>ProjectService now emits create, update, reopen, and delete events through <code>IMilestoneEventHandler</code>; state snapshots and actor/tenant context are included. The solution build passed.</td>
      <td>Added <code>docs/PROJECT-EVENT-CONSISTENCY.md</code> documenting shared-DbContext atomicity, the outbox trade-off, and the temporary project-to-milestone compatibility mapping. Missing event handling fails closed.</td>
    </tr>
    <tr>
      <td>14</td>
      <td><pre>@workspace Generate xUnit tests for the Project, Audit, and Notification behavior.

Include at least these required cases:
1. Notifications are dispatched to every relevant team member after a project milestone state change.
2. An audit entry is created correctly when a project milestone is updated.
3. An audit entry cannot be deleted or overwritten.
4. Audit history is correctly filtered by date range.
5. Audit history filtered by event type returns only matching entries.
6. An unauthorized user cannot access another organisation's audit log.

Also add:
7. A user cannot mark another user's notification as read.
8. Invalid date range is rejected.
9. Invalid status transition is rejected.
10. Tenant-scoped project lookup does not disclose another tenant's project.
11. Notification query returns unread notifications only.
12. CancellationToken is propagated where practical.

Use deterministic test data and a fake IClock. Avoid tests that depend on execution order or the local machine clock. Do not weaken production visibility only to make tests pass.</pre></td>
      <td>Agent / xUnit test generation</td>
      <td>Current Project, Audit, Notification, repository, event-handler, tenant-context, and clock abstractions.</td>
      <td>Deterministic behavior tests with in-memory contract fakes and explicit tenant/cancellation assertions.</td>
      <td>To cover the required security, consistency, filtering, lifecycle, and asynchronous behavior without changing production visibility.</td>
      <td>Twelve xUnit tests were added and the complete solution build passed.</td>
      <td>Test execution was attempted but remains blocked by the container missing the .NET 8 runtime; the test project and assembly compile successfully.</td>
    </tr>
    <tr>
      <td>15</td>
      <td><pre>@workspace Run restore, build, and all tests.

For any failure:
1. Show the exact build or test error.
2. Identify the root cause.
3. Propose the smallest valid correction.
4. Do not remove validation, authorization, tenant filtering, or assertions merely to make tests pass.
5. Apply the correction only after showing the diff.
6. Re-run the affected tests, then the complete suite.

Summarize the final build and test results without inventing results that were not produced by the terminal.</pre></td>
      <td>Agent / validation and test remediation</td>
      <td>Complete TaskBridge solution, test suite, installed .NET runtimes, and the prior event-handler integration.</td>
      <td>Failure-first validation with environment diagnosis and minimal test-only correction.</td>
      <td>To distinguish environment failures from actual test failures and verify the corrected suite without weakening production safeguards.</td>
      <td>After installing the missing user-local .NET 8 runtimes, the affected project and complete suite both passed: 16 total, 0 failed, 16 succeeded, 0 skipped.</td>
      <td>Updated the pre-existing ProjectService test factory with a no-op event-handler test double after showing the diff; production code and assertions were not weakened.</td>
    </tr>
    <tr>
      <td>16</td>
      <td><pre>@workspace Act as a senior solution architect and privacy-focused security reviewer.

A scope change has been received:

"Add a new milestone event type: MILESTONE_REOPENED. This should trigger audit logging and notifications. Audit entries must now also capture the actor's IP address."

Before modifying code, analyse the impact.

List:
- every affected file, module, model, enum, DTO, database configuration, migration, service, endpoint, validator, test, API contract, document, and logging concern,
- whether each change is additive, breaking, or needs migration,
- backward-compatibility impact,
- handling of existing audit rows,
- privacy and compliance risk of storing IP addresses,
- retention, masking, encryption, access-control, and logging-exposure concerns,
- proxy and forwarded-header trust risks,
- recommended implementation order,
- rollback considerations,
- additional tests.

Do not change any code. Do not state that an IP address reliably identifies a person.</pre></td>
      <td>Agent / privacy and security impact analysis</td>
      <td>Verified event enum, Project Service, audit/notification services, API controller, EF configuration, specifications, consistency document, and tests.</td>
      <td>Workspace-grounded change-impact analysis with privacy and compatibility boundaries.</td>
      <td>To distinguish existing reopen support from net-new IP-address collection and identify the security, migration, and operational consequences before implementation.</td>
      <td><code>docs/IMPACT_ANALYSIS.md</code> was created with the requested impact matrix, migration requirements, compatibility, privacy risks, implementation sequence, validation plan, rollback considerations, and Copilot provenance.</td>
      <td>No code was changed; the document explicitly records that no implementation correction occurred.</td>
    </tr>
    <tr>
      <td>17</td>
      <td><pre>@workspace Implement the approved change described in docs/IMPACT_ANALYSIS.md.

Add:
- MILESTONE_REOPENED event handling,
- ActorIpAddress on new audit entries,
- the required EF Core migration,
- updated contracts and validators,
- audit and notification behavior for reopened milestones,
- tests for reopened events,
- tests for IP capture,
- a test confirming old rows remain readable if ActorIpAddress is nullable.

Security constraints:
- obtain the IP address from trusted server request context, not the request body,
- account for forwarded headers only when trusted proxies are configured,
- do not include raw IP addresses in application logs,
- restrict IP visibility in API responses according to docs/SPEC.md,
- document the retention decision,
- do not modify existing audit entries.

Show all proposed changes before applying them.</pre></td>
      <td>Agent / privacy-sensitive implementation</td>
      <td><code>docs/IMPACT_ANALYSIS.md</code>, current event enum, audit model/service, API request context, EF configuration, migration tooling, and existing tests.</td>
      <td>Proposed-diff-first implementation with nullable schema evolution, trusted proxy handling, privacy-safe DTOs, and deterministic tests.</td>
      <td>To implement the approved reopen/IP scope while preserving existing audit rows and preventing client-controlled or logged IP data.</td>
      <td>Trusted IP capture, nullable audit storage, additive EF migration, reopen/IP/null-history tests, forwarded-header configuration, retention documentation, and contract propagation were added. The complete suite passed: 19 succeeded, 0 failed, 0 skipped.</td>
      <td>The generated migration initially contained the full schema because no prior migrations existed; it was corrected to an additive nullable-column migration before validation. One test fixture was corrected to commit staged audit data before asserting persistence.</td>
    </tr>
    <tr>
      <td>18</td>
      <td><pre>@workspace Act as an adversarial application security reviewer.

Review the completed TaskBridge solution for:
- tenant ID spoofing,
- insecure direct object references,
- missing project-membership checks,
- mass assignment,
- authentication bypass,
- weak internal endpoint protection,
- sensitive audit snapshot exposure,
- raw IP logging,
- unsafe forwarded-header handling,
- unrestricted audit queries,
- invalid date ranges,
- enum manipulation,
- excessive notification disclosure,
- SQL injection,
- race conditions,
- replay or duplicate audit creation,
- missing input limits,
- exception detail leakage.

For each confirmed issue, provide exact evidence, severity, attack scenario, and the smallest safe fix. Clearly state when a potential concern is already mitigated.</pre></td>
      <td>Agent / adversarial security review</td>
      <td>Completed API, application, domain, persistence, migration, configuration, and test artifacts.</td>
      <td>Evidence-first threat review separating confirmed findings from mitigated concerns.</td>
      <td>To identify exploitable tenant, authorization, privacy, availability, and data-integrity risks without inventing unsupported findings.</td>
      <td>Confirmed findings and mitigations were reported from the workspace; no implementation files were changed.</td>
      <td>None; the review remained read-only apart from this prompt-log entry.</td>
    </tr>
    <tr>
      <td>19</td>
      <td><pre>@workspace Apply only the confirmed fixes from the latest security review.

Preserve existing functionality and tests. Do not introduce a broad refactor. For each file:
- show the diff,
- relate it to the confirmed finding,
- add or update a regression test,
- preserve tenant-scoped behavior,
- avoid logging secrets, snapshots, or IP addresses.</pre></td>
      <td>Agent / targeted security remediation</td>
      <td>Latest adversarial review findings, audit/API contracts, existing tests, and tenant-scoped repository behavior.</td>
      <td>Minimal finding-to-fix diffs with regression tests and explicit unresolved dependencies.</td>
      <td>To remediate only locally actionable confirmed issues without inventing project-membership or deployment identity data.</td>
      <td>Removed raw audit snapshots and IP from ordinary responses, enforced a 90-day audit query window, stabilized validation error responses, and added regression tests. Affected and complete suites passed: 22 succeeded, 0 failed, 0 skipped.</td>
      <td>Project-membership authorization, issuer/audience configuration, cross-service payload relationship validation, and concurrent uniqueness handling remain documented as requiring missing domain/deployment inputs or broader infrastructure changes.</td>
    </tr>
    <tr>
      <td>20</td>
      <td><pre>@workspace Design the remediation of the Project Service before changing code.

Use:
- Project aggregate and ProjectStatus enum,
- request and response DTOs,
- FluentValidation,
- IProjectRepository,
- ProjectRepository with EF Core,
- IProjectService,
- ProjectService containing business rules only,
- ProjectsController,
- ICurrentUserContext exposing authenticated UserId and OrganisationId,
- structured logging,
- async methods with CancellationToken,
- tenant-scoped repository operations,
- explicit status transition validation,
- specific application exceptions,
- UTC timestamps through IClock.

Explain:
1. Responsibilities of each layer.
2. Method signatures.
3. Tenant-isolation rules.
4. Valid status transitions.
5. Delete behavior and audit implications.
6. Transaction boundary.
7. How other services will learn about milestone changes.

Do not change files yet.</pre></td>
      <td>Agent / architecture design</td>
      <td>@workspace repository state, existing Project contracts, and Copilot instructions.</td>
      <td>Constraint checklist and responsibility decomposition.</td>
      <td>To establish a safe remediation boundary before code changes.</td>
      <td>A design specifying layers, interfaces, tenant rules, transitions, transactions, and event propagation was produced; no files were changed by that prompt.</td>
      <td>None.</td>
    </tr>
    <tr>
      <td>21</td>
      <td><pre>@workspace Rewrite the Project Service according to the agreed design and .github/copilot-instructions.md.

Create or update the model, DTOs, validators, repository interface, EF Core repository, service interface, service implementation, controller, current-user context abstraction, application exceptions, and dependency-injection registration.

Mandatory rules:
- Every project belongs to an OrganisationId.
- Never accept OrganisationId or actor identity from request bodies.
- Scope all reads, updates, and deletes by project ID plus authenticated OrganisationId.
- Return DTOs rather than EF Core entities.
- Use EF Core only in Infrastructure.
- No database imports in ProjectService.
- Use async methods and CancellationToken.
- Validate status transitions.
- Return not-found semantics without revealing whether another tenant owns the project.
- Add structured logs without secrets or complete object snapshots.
- Add XML documentation to public APIs.
- Do not change the preserved initial-generated-code copies.
- Show the proposed diff before applying changes.</pre></td>
      <td>Agent / implementation with proposed diff</td>
      <td>@workspace source tree, `.github/copilot-instructions.md`, preserved evidence, and existing Project layers.</td>
      <td>Targeted remediation with explicit non-goals and diff-first review.</td>
      <td>To implement the agreed service boundaries without changing preserved evidence.</td>
      <td>Project service/current-user abstractions, transition exception, logging, and DI changes were applied; the solution built successfully.</td>
      <td>Removed redundant compatibility-interface members after compiler warnings and restored logging dependencies before the successful build.</td>
    </tr>
    <tr>
      <td>22</td>
      <td><pre>@workspace Implement only the domain models and persistence layer for the Notification and Audit Service according to docs/SPEC.md.

Create:
- AuditEntry,
- Notification,
- MilestoneEventType,
- required value objects or DTOs,
- EF Core configurations,
- repository interfaces,
- EF Core repository implementations,
- DbContext changes.

AuditEntry must capture:
- EventType,
- EntityType,
- EntityId,
- ActorUserId,
- OrganisationId,
- PreviousState,
- NewState,
- TimestampUtc.

Notification must capture:
- RecipientUserId,
- OrganisationId,
- EventType,
- ProjectId,
- Message,
- IsRead,
- CreatedAtUtc.

Mandatory:
- no update or delete repository methods for AuditEntry,
- tenant-scoped queries,
- UTC timestamps,
- appropriate indexes,
- valid maximum lengths,
- snapshots stored safely as JSON or another documented representation,
- no service or controller implementation yet.

Show proposed changes before applying them.</pre></td>
      <td>Agent / persistence implementation with proposed changes</td>
      <td>@workspace `docs/SPEC.md`, existing DbContext, Project repository, and application abstractions.</td>
      <td>Layered vertical slice and append-only contract design.</td>
      <td>To establish storage contracts before adding application behavior.</td>
      <td>Domain entities, repositories, EF configurations, DbContext sets, and unit-of-work support were added; no controllers or tests were added in that step. The solution built successfully.</td>
      <td>The generated migration was later corrected from a full-schema migration to an additive audit-column migration.</td>
    </tr>
    <tr>
      <td>23</td>
      <td><pre>Explain exactly what this selected code does, which inputs reach the database operation, what trust assumptions it makes, and whether it can read or modify another organisation's project. Do not propose a rewrite yet.</pre></td>
      <td>Chat / code explanation</td>
      <td>Selected-code context was unavailable in the workspace message.</td>
      <td>Data-flow and trust-boundary tracing request.</td>
      <td>To understand database inputs and tenant-crossing behavior before proposing changes.</td>
      <td>Copilot requested the missing code selection/file and method; no code analysis or file change was performed from that prompt.</td>
      <td>None.</td>
    </tr>
    <tr>
      <td>24</td>
      <td><pre>@workspace Compare the generated Project Service against .github/copilot-instructions.md.

Do not modify files.

List every confirmed deviation with:
- file and method,
- violated project rule,
- risk,
- recommended remediation.

Pay particular attention to repository separation, DTO boundaries, OrganisationId filtering, tenant ownership checks, audit integration, validation, structured logging, async methods, CancellationToken, and exception handling.</pre></td>
      <td>Agent / workspace comparison review</td>
      <td>@workspace ProjectService, repository, DTOs, validators, API setup, and `.github/copilot-instructions.md`.</td>
      <td>Rule-to-code comparison with confirmed deviations only.</td>
      <td>To compare generated behavior against repository-specific engineering standards without editing files.</td>
      <td>Confirmed deviations and compliant areas were listed; no implementation files were changed.</td>
      <td>None.</td>
    </tr>
    <tr>
      <td>25</td>
      <td><pre>@workspace Complete docs/PROMPTS.md using the prompts actually used in this session.

For each prompt include:
- exact prompt text,
- mode or feature,
- #file, @workspace, slash command, Inline Chat, or terminal context used,
- prompting technique,
- rationale,
- result,
- post-generation correction.

Ensure the file demonstrates:
- at least two different Copilot modes or features,
- at least three prompting techniques,
- at least one #file or @workspace reference.

Finish with "Post-Generation Corrections".

In that section, record every actual correction made to Copilot output, what was wrong, and whether it was fixed by follow-up prompt, Edit Mode, /fix, or manual editing.

Do not invent interactions, failures, or corrections.</pre></td>
      <td>Agent / documentation audit</td>
      <td>Existing `docs/PROMPTS.md`, complete session transcript, verified workspace files, and terminal results.</td>
      <td>Audit-log completion with exact provenance and correction accounting.</td>
      <td>To make the prompt record complete without inventing interactions or outcomes.</td>
      <td>This file was updated with omitted session prompts, exact context labels, verified results, and a corrections section.</td>
      <td>Historical entries were supplemented where earlier logging omitted actual prompts; no implementation result was fabricated.</td>
    </tr>
    <tr>
      <td>26</td>
      <td><pre>@workspace Draft docs/PR_DESCRIPTION.md based only on the completed code, test results, and documentation.

Include:
- Summary,
- Why the change was needed,
- Project Service remediation,
- Notification and Audit implementation,
- Integration contract,
- AI Tool Disclosure,
- specific Copilot features used,
- where Copilot output was accepted,
- where it was changed or rejected,
- honest estimated percentage of AI-generated versus hand-written code,
- effect of .github/copilot-instructions.md,
- Testing,
- Known gaps,
- Risks and trade-offs,
- Self-review checklist,
- Peer Review Simulation.

Under Peer Review Simulation, add three specific, actionable, and constructive comments with file and method locations. At least one must address a real business, integration, operational, or multi-tenant edge case that AI may overlook.

Do not invent test coverage percentages or claim checks passed unless verified.</pre></td>
      <td>Agent / PR documentation</td>
      <td>@workspace completed code, verified terminal results, specifications, reviews, impact analysis, and prompt log.</td>
      <td>Evidence-constrained PR synthesis with explicit AI provenance and simulated peer review.</td>
      <td>To describe the completed change honestly without inventing coverage or validation outcomes.</td>
      <td><code>docs/PR_DESCRIPTION.md</code> was created with all requested sections, verified test results, known gaps, risks, and three actionable peer-review comments.</td>
      <td>None; the document records estimates and unresolved gaps explicitly rather than presenting them as completed work.</td>
    </tr>
    <tr>
      <td>27</td>
      <td><pre>@workspace Create docs/ARCHITECTURE.md in 10 to 15 concise lines.

Cover:
- relationship between Project Service and Notification and Audit Service,
- integration contract,
- controller -> service -> repository -> model flow,
- authenticated tenant context,
- audit and notification persistence,
- immutable audit records,
- why the architecture suits multi-tenant B2B SaaS,
- consistency boundary,
- key decisions,
- one genuine trade-off.

Base every statement on the implemented solution.</pre></td>
      <td>Agent / concise architecture documentation</td>
      <td>@workspace implemented Project, Audit, Notification, repository, tenant-context, and unit-of-work code.</td>
      <td>Constraint-based synthesis with implementation-grounded statements only.</td>
      <td>To provide a compact architecture reference without introducing planned-but-unimplemented behavior.</td>
      <td><code>docs/ARCHITECTURE.md</code> was created in 14 concise numbered lines covering all requested areas.</td>
      <td>None.</td>
    </tr>
    <tr>
      <td>28</td>
      <td><pre>@workspace Produce a Mermaid architecture and data-flow diagram for README.md.

Show:
- authenticated client,
- ASP.NET Core controllers,
- current-user context,
- Project application service,
- Project repository,
- Project database,
- milestone event or integration contract,
- Audit service,
- Notification service,
- Audit repository,
- Notification repository,
- persistence,
- tenant-boundary checks.

Show the flow for a project milestone update through project persistence, audit append, and notification creation. Match the actual implementation and do not add components that do not exist.</pre></td>
      <td>Agent / README architecture documentation</td>
      <td>@workspace implemented controllers, current-user context, ProjectService, repositories, event handler, services, DbContext, and SQLite configuration.</td>
      <td>Implementation-grounded Mermaid flow modeling with explicit tenant-boundary nodes.</td>
      <td>To make the actual milestone update and shared persistence flow visible without introducing unimplemented components.</td>
      <td>The Mermaid architecture and data-flow diagram was added to <code>README.md</code> and names the implemented contracts and persistence components.</td>
      <td>None.</td>
    </tr>
    <tr>
      <td>29</td>
      <td><pre>@workspace Create or update README.md.

Include:
- project purpose,
- technology stack,
- prerequisites,
- solution structure,
- local setup,
- database migration command,
- run command,
- test command,
- authentication and tenant-context assumptions,
- required endpoints,
- example requests without secrets or real PII,
- architecture diagram,
- important design decisions,
- limitations and trade-offs,
- documentation-file index.

Verify all commands against the actual solution before writing them.</pre></td>
      <td>Agent / README documentation</td>
      <td>@workspace README, solution, project files, appsettings, controllers, documentation, and terminal command results.</td>
      <td>Verified-command-first documentation with implementation-grounded examples.</td>
      <td>To make the repository usable without documenting commands or API shapes that were not checked.</td>
      <td><code>README.md</code> was expanded with the requested setup, migration, run, test, API, architecture, design, limitation, and documentation-index sections. Restore, build, tests, migration listing, EF command help, and API startup were verified.</td>
      <td>Corrected the audit example so snapshot values are JSON strings, matching the implemented DTO contract.</td>
    </tr>
    <tr>
      <td>30</td>
      <td><pre>@workspace Review all public classes, interfaces, controllers, methods, request types, and response types.

Add concise XML documentation only where required by .github/copilot-instructions.md. Document behavior, tenant scope, exceptions, and cancellation where relevant.

Do not add comments that merely repeat the code. Do not expose sensitive implementation details.</pre></td>
      <td>Agent / documentation review</td>
      <td>@workspace public declarations across Domain, Application, Infrastructure, API, and `.github/copilot-instructions.md`.</td>
      <td>Targeted public-API documentation audit with minimal XML additions.</td>
      <td>To satisfy the repository documentation standard without adding repetitive or sensitive comments.</td>
      <td>Public request, event, query, response, and internal audit contract fields received concise XML parameter documentation. Build and all 22 tests passed.</td>
      <td>None; documentation-only changes preserved behavior and tenant/privacy boundaries.</td>
    </tr>
    <tr>
      <td>31</td>
      <td><pre>@workspace Review the final changed files and propose at least five logical Conventional Commits in chronological order.

The history should tell this story:
1. standards and project setup,
2. preserved AI-generated Project Service and review,
3. Project Service remediation,
4. specification and Notification and Audit implementation,
5. tests,
6. impact analysis and reopened-milestone change,
7. remaining documentation.

For each commit provide:
- Conventional Commit subject,
- descriptive body,
- exact files to include.

Do not claim that commits already exist.</pre></td>
      <td>Agent / workspace history planning</td>
      <td>@workspace final file inventory, completed documentation, source, infrastructure, API, and tests.</td>
      <td>Chronological narrative grouping with exact path ownership and no commit-state assumptions.</td>
      <td>To make the assessment history reviewable without creating commits or claiming nonexistent history.</td>
      <td>Seven logical Conventional Commit proposals were prepared in chronological order; no commits were created.</td>
      <td>None.</td>
    </tr>
  </tbody>
</table>

## Maintenance Rule

Append each later implementation, review, testing, or documentation prompt as a new table row. Preserve the prompt wording exactly, record the Copilot mode or feature used, and only describe a result as successful when the resulting workspace files or response can be verified.

## Post-Generation Corrections

The following corrections actually occurred in this session:

| Correction | What was wrong | How it was corrected |
|---|---|---|
| Solution scaffolding | The installed SDK generated only `net10.0` templates and defaulted to `.slnx`, while the request required .NET 8 and `TaskBridge.sln`. | Manual editing and terminal commands retargeted projects to `net8.0` and created the classic solution format. |
| EF package compatibility | EF Core package resolution selected version 10, incompatible with `net8.0`. | Follow-up terminal command pinned EF Core SQLite and Design to `8.0.11`. |
| Missing test runtime | The container initially lacked .NET 8 runtimes. | Follow-up terminal commands installed the .NET 8 runtime and ASP.NET Core runtime under `$HOME/.dotnet`. |
| Existing ProjectService tests | Three existing tests did not provide the newly required event-handler dependency and failed closed with `ProjectEventHandlerNotConfiguredException`. | A shown test-only diff added a no-op event-handler double; production validation and assertions were preserved. |
| Test unit-of-work assertion | The IP-capture test inspected committed entries before committing staged audit data. | A shown test correction called the fake repository commit before asserting persistence. |
| OpenAPI namespace | The installed OpenAPI package did not provide `Microsoft.OpenApi.Models`. | Manual editing changed the import to the namespace provided by the installed package. |
| JWT package | `AddJwtBearer` was unavailable because the API project lacked the JWT bearer package. | A shown package change added `Microsoft.AspNetCore.Authentication.JwtBearer` version `8.0.11`. |
| Compatibility-interface warnings | `ICurrentTenant` redundantly redeclared members inherited from `ICurrentUserContext`. | Manual editing removed the duplicate declarations while retaining the compatibility interface. |
| Migration shape | EF generated a full-schema migration because no prior migrations existed, which was unsafe for an existing schema. | Manual editing changed it to an additive nullable `ActorIpAddress` column migration. |
| Unread notification contract | Notification retrieval initially exposed an `UnreadOnly` query flag even though the requirement mandated unread-only results. | Manual editing removed the flag and enforced `!notification.IsRead` in the repository query. |
| Prompt-log result count | The security-remediation entry recorded 21 tests before a final controller regression test was added. | Manual editing corrected the verified result to 22 passing tests. |

No `/fix` command or Edit Mode interaction was used. Corrections were made through follow-up prompts, `apply_patch`, package/CLI commands, and manual workspace edits as recorded above.
