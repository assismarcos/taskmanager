# T32 — TasksController

- Phase: 3 — API
- Depends on: T30
- Agent: backend
- Design: `docs/design/DD-02-task-management.md`

## Goal
Expose the Tasks API (`/api/tasks`) exactly as specified, as a thin controller over `ITaskService`.

## Context
- Spec: `docs/spec/user-story.md` — AC-3 to AC-9
- Design: DD-02 sections 3 and 6
- ADRs: ADR-0004, ADR-0006
- Contracts: `ITaskService` and DTOs (on `main`); `Result` → HTTP mapping (from T30)

## Scope — files you may create or change
- `backend/src/TaskManager.Api/Controllers/TasksController.cs`
- `backend/tests/TaskManager.Api.UnitTests/Controllers/TasksControllerTests.cs`

## Out of scope
- `Program.cs` (orchestrator), Application services, `AuthController` (T31)

## Tests to write first (red) — from DD-02, section 6
- `Create_WhenSuccessful_Returns201WithLocation`
- `Create_WhenValidationFails_Returns400ValidationProblem`
- `GetById_WhenNotFound_Returns404`
- `Update_WhenSuccessful_Returns200`
- `Delete_WhenSuccessful_Returns204`
- `List_ReturnsPagedResponse`

Attributes: `[ApiExplorerSettings(GroupName = "tasks-v1")]`, routes with `{id:guid}`, `CreatedAtAction(nameof(GetById), …)` for the `Location` header, `ProducesResponseType` for every status in DD-02 section 3. No `[AllowAnonymous]` anywhere (AC-9).

## Definition of done
- [ ] All tests above written first and seen failing, then passing
- [ ] No business logic in the controller; the user id is never read from the request
- [ ] Zero build warnings; `dotnet format` clean; architecture tests green
- [ ] Only files inside Scope changed
- [ ] Conventional commits: `test(api)` → `feat(api)` → `refactor(api)`
- [ ] Final report: files changed, tests added, decisions taken, open questions
