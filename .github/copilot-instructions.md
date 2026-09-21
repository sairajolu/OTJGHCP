# TaskBridge Copilot Instructions

## Technology

- Use .NET 8 and C#.
- Use ASP.NET Core Web API for HTTP endpoints.
- Use Entity Framework Core with SQLite for persistence.
- Use xUnit and FluentAssertions for tests.
- Use FluentValidation for request validation.
- Use Serilog for structured logging.

## Architecture

- Preserve the dependency flow: controller -> application service -> repository -> EF Core model.
- Controllers contain HTTP concerns only, including model binding, status codes, and response mapping.
- Application services contain business logic only.
- Repositories contain persistence logic only.
- Do not use raw SQL or database-driver calls in application services.
- Prefer asynchronous methods and pass `CancellationToken` through the complete call chain.
- Use dependency injection and abstractions expressed as interfaces.
- Do not expose EF Core entities as API responses; map them to explicit response DTOs.

## Coding Standards

- Keep nullable reference types enabled and resolve nullable warnings rather than suppressing them.
- Define explicit request and response DTOs for API contracts.
- Use PascalCase for public members and camelCase for local variables and parameters.
- Keep methods small and focused on one responsibility.
- Add XML documentation to public types and public methods.
- Throw specific domain or application exceptions instead of generic `Exception`.
- Never add empty `catch` blocks; handle, transform, log, or rethrow exceptions meaningfully.

## Multi-Tenancy and Security

- Every tenant-owned entity must include `OrganisationId`.
- Obtain the actor identity and `OrganisationId` from an authenticated request context. Never trust tenant identity supplied by a client request.
- Every tenant-owned repository query must filter by `OrganisationId`.
- Prevent cross-organisation reads, updates, deletes, and relationship creation.
- Validate identifiers, enum values, date ranges, and input lengths at the application boundary.
- Never log secrets, tokens, complete request bodies, snapshots containing sensitive data, or raw IP addresses.
- Use structured logging with named properties rather than interpolated log messages.
- Never hardcode credentials, secrets, or personally identifiable information.

## Auditing

- Treat audit records as append-only.
- Do not expose update or delete operations for audit entries.
- Record before and after state where applicable, while excluding or redacting sensitive data.
- Configure and use EF Core in a way that prevents accidental modification of existing audit records.

## Testing

- Test happy paths, validation failures, not-found conditions, tenant isolation, and authorization.
- Test audit immutability, date filtering, event filtering, and notification dispatch.
- Use deterministic UTC timestamps through an injectable clock abstraction rather than calling system time directly in business logic.
- Keep tests isolated, repeatable, and explicit about organisation and actor context.

## Ambiguity Rule

When a requirement is ambiguous, prefer the safest multi-tenant behavior and document the assumption.