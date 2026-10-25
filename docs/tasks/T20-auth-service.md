# T20 — AuthService and validators (Application)

- Phase: 2 — Use cases
- Depends on: T10
- Agent: backend
- Design: `docs/design/DD-01-authentication.md`

## Goal
Implement registration, login and current-user use cases with their validators, using only the Application ports.

## Context
- Spec: `docs/spec/user-story.md` — AC-1, AC-2, AC-9 and the business rules table
- Design: DD-01 sections 2, 3, 5 and 6
- ADRs: ADR-0005, ADR-0006, ADR-0007
- Contracts: `IAuthService`, DTOs, `IUserRepository`, `IPasswordHasher`, `ITokenService`, `ICurrentUser` (on `main`)
- Normalize the email exactly as `User.Create` does (`Trim().ToLowerInvariant()`) before `GetByEmailAsync`, so login and registration agree.

## Scope — files you may create or change
- `backend/src/TaskManager.Application/Auth/**`
- `backend/tests/TaskManager.Application.UnitTests/Auth/**`
- `backend/tests/TaskManager.Application.UnitTests/Builders/UserBuilder.cs` (T21 owns `TaskItemBuilder.cs` in the same folder)

## Out of scope
- Controllers, EF Core, JWT and hashing implementations (Infrastructure), `DependencyInjection.cs` (orchestrator)

## Tests to write first (red) — from DD-01, section 6
- `Register_WithValidRequest_ReturnsUserResponse` (AC-1)
- `Register_WhenEmailIsTaken_ReturnsConflict` (AC-1)
- `Register_HashesPasswordBeforeSaving` (AC-1)
- `RegisterValidator_WhenPasswordHasNoDigit_ReturnsValidationError` (AC-1)
- `RegisterValidator_WhenPasswordIsShorterThan8Chars_ReturnsValidationError` (AC-1)
- `RegisterValidator_WhenEmailIsInvalid_ReturnsValidationError` (AC-1)
- `Login_WithValidCredentials_ReturnsAccessToken` (AC-2)
- `Login_WhenEmailIsUnknown_ReturnsInvalidCredentials` (AC-2)
- `Login_WhenPasswordIsWrong_ReturnsInvalidCredentials` (AC-2)
- `Login_WithMixedCaseEmail_FindsUser` (AC-2)
- `GetCurrentUser_ReturnsUserFromToken` (AC-9)
- `GetCurrentUser_WhenUserNoLongerExists_ReturnsUnauthorized` (AC-9)

Mock ports with NSubstitute; use `FakeTimeProvider` for `now`. Assert on behavior (returned `Result`, saved user), not on call counts, unless the call is the behavior (the password is hashed).

## Definition of done
- [ ] All tests above written first and seen failing, then passing (Test-Driven Development)
- [ ] `dotnet build backend/TaskManager.slnx` with zero warnings; `dotnet format` clean
- [ ] Architecture tests still green
- [ ] Only files inside Scope changed
- [ ] Conventional commits: `test(app)` → `feat(app)` → `refactor(app)`
- [ ] Final report: files changed, tests added, decisions taken, open questions
