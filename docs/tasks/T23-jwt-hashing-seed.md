# T23 — JWT token service, password hashing and demo seed (Infrastructure)

- Phase: 2 — Use cases
- Depends on: T11
- Agent: backend
- Design: `docs/design/DD-01-authentication.md`, `docs/design/DD-04-runtime-and-quality.md`

## Goal
Implement `ITokenService` and `IPasswordHasher`, the validated `JwtOptions`, and the database initializer that migrates and seeds demo data in Development.

## Context
- Spec: `docs/spec/user-story.md` — AC-2 (token), business rules (60-minute token)
- Design: DD-01 sections 2, 3 and 5; DD-04 sections 2, 3 and 4 (seed data table)
- ADRs: ADR-0005, ADR-0003
- Contracts: `ITokenService`, `AccessToken`, `IPasswordHasher` (on `main`); `PostgresFixture` (from T11)
- Seed test classes follow the T11 pattern: `[Collection("Postgres")]`, `[Trait("Category", "Integration")]`, `IAsyncLifetime` with `InitializeAsync() => fixture.ResetAsync()`, contexts from `fixture.CreateContext()`. Tokens use `JsonWebTokenHandler` (`Microsoft.IdentityModel.JsonWebTokens`, already referenced); hashing wraps `PasswordHasher<User>` (`Microsoft.Extensions.Identity.Core`).

## Scope — files you may create or change
- `backend/src/TaskManager.Infrastructure/Authentication/**` (`JwtOptions`, `JwtTokenService`, `PasswordHasher` adapter over `PasswordHasher<User>`)
- `backend/src/TaskManager.Infrastructure/Persistence/Seed/**` (`DatabaseInitializer`: migrate + idempotent seed)
- `backend/tests/TaskManager.Infrastructure.IntegrationTests/Authentication/**`
- `backend/tests/TaskManager.Infrastructure.IntegrationTests/Seed/**`

## Out of scope
- Repositories (T22), `Program.cs` and JwtBearer validation setup (T30), `DependencyInjection.cs` (orchestrator)

## Tests to write first (red) — from DD-01 and DD-04, section 6
- `CreateToken_ContainsSubEmailAndNameClaims` (AC-2)
- `CreateToken_ExpiresAfterConfiguredMinutes` (AC-2, uses `FakeTimeProvider`)
- `Hash_ThenVerifyWithSamePassword_ReturnsTrue` (AC-1, AC-2)
- `Verify_WithWrongPassword_ReturnsFalse` (AC-2)
- `JwtOptions_WithKeyShorterThan32Chars_FailsValidation`
- `Seed_OnEmptyDatabase_CreatesDemoUsersAndTasks` (Integration)
- `Seed_RunTwice_DoesNotDuplicateData` (Integration)
- `Seed_CreatesAtLeastOneOverdueTaskForDemoUser` (Integration)

Token and hashing tests do not need Docker and carry no `Integration` trait. Overdue seed tasks use `TaskItem.Create(…, now: a past date)` (DD-04 section 4).

## Definition of done
- [ ] All tests above written first and seen failing, then passing
- [ ] No key, password or token is logged or hard-coded outside the seed's documented demo passwords
- [ ] Zero build warnings; `dotnet format` clean; architecture tests green
- [ ] Only files inside Scope changed
- [ ] Conventional commits: `test(infra)` → `feat(infra)` → `refactor(infra)`
- [ ] Final report: files changed, tests added, decisions taken, open questions
