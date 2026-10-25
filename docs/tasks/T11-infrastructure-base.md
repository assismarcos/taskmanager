# T11 — Infrastructure base: AppDbContext, mappings, migration, PostgresFixture

- Phase: 1 — Core
- Depends on: T02
- Agent: backend
- Design: `docs/design/DD-02-task-management.md`, `docs/design/DD-01-authentication.md`, `docs/design/DD-04-runtime-and-quality.md`

## Goal
Map `User` and `TaskItem` to PostgreSQL, create the initial migration, and build the Testcontainers fixture every integration test will reuse.

## Context
- Spec: `docs/spec/user-story.md` — AC-1 (unique email), AC-3 (ownership via FK)
- Design: DD-01 section 4; DD-02 section 4; DD-04 section 6 (fixture design)
- ADRs: ADR-0003, ADR-0008
- Contracts: entity shapes in `TaskManager.Domain` (from T02)

## Scope — files you may create or change
- `backend/src/TaskManager.Infrastructure/Persistence/AppDbContext.cs`
- `backend/src/TaskManager.Infrastructure/Persistence/DesignTimeDbContextFactory.cs` (so `dotnet ef` works before the Api host exists)
- `backend/src/TaskManager.Infrastructure/Persistence/Configurations/**`
- `backend/src/TaskManager.Infrastructure/Persistence/Migrations/**`
- `backend/tests/TaskManager.Infrastructure.IntegrationTests/Fixtures/**`
- `backend/tests/TaskManager.Infrastructure.IntegrationTests/Persistence/**`

## Out of scope
- Repositories (T22), JWT, hashing and seed (T23), `DependencyInjection.cs` (orchestrator)
- `Directory.Packages.props`: every package is already listed. If one is missing, stop and report.

## Tests to write first (red) — from DD-01, DD-02 and DD-04, section 6
- `Migrations_ApplyToEmptyDatabase`
- `Users_WithDuplicateEmail_ViolateUniqueIndex` (AC-1)
- `Tasks_WithUnknownUserId_ViolateForeignKey` (AC-3)
- `ResetAsync_RemovesAllRows`

Every test class carries `[Trait("Category", "Integration")]` and joins the `"Postgres"` collection. Migration command:
`dotnet ef migrations add InitialCreate --project backend/src/TaskManager.Infrastructure --output-dir Persistence/Migrations`

## Definition of done
- [ ] All tests above written first and seen failing, then passing (Docker running)
- [ ] snake_case names, enum as text, composite index `(user_id, status, due_date)`, cascade delete, as in DD-02 section 4
- [ ] Zero build warnings; `dotnet format` clean; architecture tests green
- [ ] Only files inside Scope changed
- [ ] Conventional commits: `test(infra)` → `feat(infra)` → `refactor(infra)`
- [ ] Final report: files changed, tests added, decisions taken, open questions
