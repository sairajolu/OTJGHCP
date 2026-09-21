# TaskBridge Architecture
1. `ProjectService` owns Project lifecycle rules and emits `MilestoneChangedEvent` through `IMilestoneEventHandler`.
2. The event handler coordinates `AuditService`, `NotificationService`, recipient resolution, and one shared `IUnitOfWork`.
3. API controllers handle HTTP binding and exception mapping; application services hold business orchestration.
4. Persistence follows controller -> application service -> repository -> EF Core model.
5. `ICurrentUserContext` supplies authenticated `UserId`, `OrganisationId`, and trusted actor IP metadata.
6. Project, audit, and notification repository queries include organisation scope; notifications also scope by recipient.
7. EF Core persists `Project`, immutable `AuditEntry`, and read-state `Notification` records in SQLite.
8. Audit repositories expose append/query operations only; existing audit records have no update or delete path.
9. DTO mappings prevent EF/domain entities, audit snapshots, and raw IP addresses from ordinary API responses.
10. The architecture suits multi-tenant B2B SaaS by making tenant context explicit at service and repository boundaries.
11. The consistency boundary is the shared DbContext/unit of work, where Project, audit, and notification writes commit together.
12. Across service boundaries, the selected direction is a transactional outbox with eventual consistency and idempotent event handling.
13. Key decisions include fail-closed event handling, UTC clocks, bounded queries, tenant filters, and trusted-proxy-only forwarded headers.
14. The genuine trade-off is that SQLite/shared transactions are simple for assessment atomicity, while production dispatch needs outbox retries and eventual consistency.
