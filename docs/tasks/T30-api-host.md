# T30 — API host: Program.cs, auth, ProblemDetails, OpenAPI, CORS, health, rate limiting

- Phase: 3 — API
- Depends on: T20, T21, T22, T23 (merged, with `AddApplication()` / `AddInfrastructure()` wired by the orchestrator)
- Agent: backend
- Design: `docs/design/DD-01-authentication.md`, `docs/design/DD-04-runtime-and-quality.md`

## Goal
Configure the ASP.NET Core host so controllers only need to call a service and return `Result.ToActionResult()`.

## Context
- Spec: `docs/spec/user-story.md` — AC-9 (secure by default), AC-3 (user id only from the token)
- Design: DD-01 sections 2, 3 and 5; DD-02 section 3 (error table); DD-04 sections 2, 3 and 5
- ADRs: ADR-0004, ADR-0005, ADR-0006

## Scope — files you may create or change
This task is the only one that edits `Program.cs`; afterwards it is reserved for the orchestrator.
- `backend/src/TaskManager.Api/Program.cs` (+ `public partial class Program;` for tests)
- `backend/src/TaskManager.Api/Http/**` (`Result` → `IActionResult` / ProblemDetails mapping, `GlobalExceptionHandler`)
- `backend/src/TaskManager.Api/Authentication/**` (`HttpContextCurrentUser`)
- `backend/src/TaskManager.Api/OpenApi/**`
- `backend/src/TaskManager.Api/appsettings*.json`, `Properties/launchSettings.json` (HTTP on 5080)
- `backend/tests/TaskManager.Api.UnitTests/Http/**`, `backend/tests/TaskManager.Api.UnitTests/Authentication/**`

## Out of scope
- Controllers (T31, T32), end-to-end tests (T33)

## What Program.cs configures
- `AddApplication()`, `AddInfrastructure()`
- JwtBearer (issuer, audience, key, lifetime, 30 s clock skew) and a fallback policy requiring an authenticated user
- `AddProblemDetails()` + `GlobalExceptionHandler`
- Two OpenAPI documents (`auth-v1`, `tasks-v1`, by `GroupName`) + Scalar at `/scalar` (anonymous)
- CORS from `Cors:AllowedOrigins`
- `/health` with a database check (anonymous)
- Rate limiter policy `login` (`RateLimiting:LoginPermitLimit` per minute per IP → 429)
- JSON enums as strings
- `TimeProvider.System`
- In Development, the database initializer from T23

## Tests to write first (red)
- `ToActionResult_ForEachErrorType_ReturnsMatchingStatus` (theory: Validation 400, NotFound 404, Conflict 409, Unauthorized 401)
- `ToActionResult_ForValidationError_ReturnsValidationProblemWithFieldErrors`
- `GlobalExceptionHandler_ForDomainException_Returns400ProblemDetails`
- `GlobalExceptionHandler_ForUnexpectedException_Returns500WithoutStackTrace`
- `HttpContextCurrentUser_ReadsSubClaim` (AC-3)

## Definition of done
- [ ] All tests above written first and seen failing, then passing
- [ ] `dotnet run` starts; `/health` 200; `/scalar` shows both documents; a request without a token to any non-public route returns 401
- [ ] Zero build warnings; `dotnet format` clean; architecture tests green
- [ ] Only files inside Scope changed
- [ ] Conventional commits: `test(api)` → `feat(api)` → `refactor(api)`
- [ ] Final report: files changed, tests added, decisions taken, open questions
