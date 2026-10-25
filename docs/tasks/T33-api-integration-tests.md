# T33 — End-to-end API integration tests

- Phase: 3 — API
- Depends on: T31, T32
- Agent: test-engineer
- Design: `docs/design/DD-04-runtime-and-quality.md`, `docs/design/DD-01-authentication.md`, `docs/design/DD-02-task-management.md`

## Goal
Prove the acceptance criteria over real HTTP, through the whole stack, against a real PostgreSQL.

## Context
- Spec: `docs/spec/user-story.md` — AC-1, AC-2, AC-3, AC-5, AC-7, AC-8, AC-9
- Design: DD-04 section 6 (fixture, factory, `AuthenticateAsync`); DD-01 and DD-02 section 6
- ADRs: ADR-0008

## Scope — files you may create or change
- `backend/tests/TaskManager.Api.IntegrationTests/**` (`PostgresFixture` copy, `CustomWebApplicationFactory`, helpers, tests)

## Out of scope
- Any production code. If a test exposes a bug, stop and report it with the failing test; do not fix it here.

## Tests to write first (red) — from DD-01 and DD-02, section 6
- `RegisterThenLogin_ReturnsToken` (AC-1, AC-2)
- `Register_WithDuplicateEmail_Returns409` (AC-1)
- `Login_WithWrongPassword_Returns401WithGenericMessage` (AC-2)
- `Me_WithoutToken_Returns401` (AC-9)
- `Me_WithToken_ReturnsUser`
- `Status_WithoutToken_Returns200`
- `TaskEndpoints_WithoutToken_Return401` (theory over all five; AC-9)
- `CreateThenGet_ReturnsSameTask` (AC-3)
- `GetById_TaskOfAnotherUser_Returns404` (AC-3)
- `Update_TaskOfAnotherUser_Returns404` (AC-3)
- `Delete_TaskOfAnotherUser_Returns404` (AC-3)
- `Create_WithPastDueDate_Returns400WithFieldError` (AC-5)
- `List_FilteredByStatus_ReturnsOnlyMatchingTasks` (AC-7)
- `List_AfterDueDatePasses_MarksTaskAsOverdue` (AC-8, advances the `FakeTimeProvider`)
- Optional (cut first): `Login_AfterTooManyAttempts_Returns429`

Red here means: write the test against the documented contract and run it. A test that passes on the first run without ever failing must be checked by breaking its assertion once.

## Definition of done
- [ ] All tests above green with Docker running; each class has `[Trait("Category", "Integration")]`
- [ ] Tests are independent (Respawn between tests) and pass in any order
- [ ] Zero build warnings; `dotnet format` clean
- [ ] Only files inside Scope changed
- [ ] Conventional commits: `test(e2e): ...`
- [ ] Final report: files changed, tests added, bugs found, open questions
