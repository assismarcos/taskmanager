# T22 — Repositories with Testcontainers tests (Infrastructure)

- Phase: 2 — Use cases
- Depends on: T11
- Agent: backend
- Design: `docs/design/DD-02-task-management.md`, `docs/design/DD-01-authentication.md`

## Goal
Implement `TaskRepository` and `UserRepository` on EF Core, with the per-user filter inside every task query, proven against a real PostgreSQL.

## Context
- Spec: `docs/spec/user-story.md` — AC-1, AC-3, AC-6, AC-7
- Design: DD-02 sections 3, 4, 5, 6 and 7; DD-01 sections 2 and 3 (`TryAddAsync` semantics)
- ADRs: ADR-0003, ADR-0007 (`DbContext` is the unit of work), ADR-0008
- Contracts: `ITaskRepository`, `IUserRepository` (on `main`); `PostgresFixture` (from T11)
- Test classes follow the T11 pattern: `[Collection("Postgres")]`, `[Trait("Category", "Integration")]`, `IAsyncLifetime` with `InitializeAsync() => fixture.ResetAsync()`, contexts from `fixture.CreateContext()`, data from `User.Create` / `TaskItem.Create` (no SQL).

## Scope — files you may create or change
- `backend/src/TaskManager.Infrastructure/Persistence/Repositories/**`
- `backend/tests/TaskManager.Infrastructure.IntegrationTests/Repositories/**`

## Out of scope
- `AppDbContext` and mappings (T11; report if a change is needed), JWT, hashing and seed (T23), `DependencyInjection.cs` (orchestrator)
- Optimistic concurrency with `xmin` (deferred, DD-02 section 7)

## Tests to write first (red) — from DD-02 and DD-01, section 6
- `List_ReturnsOnlyTasksOfGivenUser` (AC-3)
- `GetById_WhenTaskBelongsToAnotherUser_ReturnsNull` (AC-3)
- `List_FiltersByStatusAndOrdersByDueDate` (AC-7)
- `List_ReturnsTotalCountAcrossPages` (AC-7)
- `Add_StoresStatusAsText` (AC-6)
- `DeletingUser_CascadesToTasks`
- `TryAdd_WhenEmailIsTaken_ReturnsFalse` (AC-1)
- `GetByEmail_ReturnsMatchingUser` (AC-1)

Each write calls `SaveChangesAsync`. `ListAsync` uses `AsNoTracking`, orders by `due_date`, `created_at`, `id`. `TryAddAsync` translates the PostgreSQL unique violation (`23505`) into `false`.

## Definition of done
- [ ] All tests above written first and seen failing, then passing (Docker running)
- [ ] Zero build warnings; `dotnet format` clean; architecture tests green
- [ ] Only files inside Scope changed
- [ ] Conventional commits: `test(infra)` → `feat(infra)` → `refactor(infra)`
- [ ] Final report: files changed, tests added, decisions taken, open questions
