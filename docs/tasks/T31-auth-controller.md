# T31 — AuthController

- Phase: 3 — API
- Depends on: T30
- Agent: backend
- Design: `docs/design/DD-01-authentication.md`

## Goal
Expose the Auth API (`/api/auth`) exactly as specified, as a thin controller over `IAuthService`.

## Context
- Spec: `docs/spec/user-story.md` — AC-1, AC-2, AC-9
- Design: DD-01 sections 3 and 6
- ADRs: ADR-0004, ADR-0006
- Contracts: `IAuthService` and DTOs (on `main`); `Result` → HTTP mapping (from T30)

## Scope — files you may create or change
- `backend/src/TaskManager.Api/Controllers/AuthController.cs`
- `backend/tests/TaskManager.Api.UnitTests/Controllers/AuthControllerTests.cs`

## Out of scope
- `Program.cs` (orchestrator), Application services, `TasksController` (T32)

## Tests to write first (red) — from DD-01, section 6
- `Register_WhenSuccessful_Returns201WithLocation` (AC-1)
- `Register_WhenEmailIsTaken_Returns409ProblemDetails` (AC-1)
- `Login_WhenSuccessful_Returns200WithToken` (AC-2)
- `Login_WhenCredentialsAreInvalid_Returns401ProblemDetails` (AC-2)
- `Me_ReturnsCurrentUser` (AC-9)
- `Status_ReturnsOkWithVersion`

Attributes: `[ApiExplorerSettings(GroupName = "auth-v1")]`, `[AllowAnonymous]` on register, login and status, and `[EnableRateLimiting("login")]` on login. Add `ProducesResponseType` for every status in DD-01 section 3.

## Definition of done
- [ ] All tests above written first and seen failing, then passing
- [ ] No business logic in the controller: call the service, map the result
- [ ] `Controllers_ShouldNotDependOnInfrastructure` seen failing once: temporarily use an Infrastructure type in `AuthController`, run the architecture tests, see it fail naming the controller, then revert (T03 could not, because no controller existed yet)
- [ ] Zero build warnings; `dotnet format` clean; architecture tests green
- [ ] Only files inside Scope changed
- [ ] Conventional commits: `test(api)` → `feat(api)` → `refactor(api)`
- [ ] Final report: files changed, tests added, decisions taken, open questions
