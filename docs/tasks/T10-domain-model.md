# T10 — Domain: TaskItem and User behavior

- Phase: 1 — Core
- Depends on: T02
- Agent: backend
- Design: `docs/design/DD-02-task-management.md`, `docs/design/DD-01-authentication.md`

## Goal
Implement the entity invariants, status transitions and the overdue rule with Test-Driven Development. After this task, no `TaskItem` or `User` can exist in an invalid state.

## Context
- Spec: `docs/spec/user-story.md` — AC-1, AC-4, AC-5, AC-6, AC-8 and the business rules table
- Design: DD-02 sections 4, 5 and 6; DD-01 sections 4, 5 and 6
- ADRs: ADR-0002, ADR-0006
- Contracts: entity shapes in `TaskManager.Domain` (already on `main`, from T02)

## Scope — files you may create or change
- `backend/src/TaskManager.Domain/**`
- `backend/tests/TaskManager.Domain.UnitTests/**`

## Out of scope
- Application, Infrastructure, Api; NuGet packages in Domain (none allowed)
- Changing public signatures from T02. If one is wrong, stop and report.

## Tests to write first (red) — from DD-02 and DD-01, section 6
- `Create_WithValidData_SetsAllFieldsAndTimestamps`
- `Create_WhenTitleIsShorterThan3Chars_ThrowsDomainException` (AC-4)
- `Create_WhenTitleIsLongerThan100Chars_ThrowsDomainException` (AC-4)
- `Create_WhenDescriptionIsLongerThan1000Chars_ThrowsDomainException` (AC-4)
- `Create_WithWhitespaceDescription_StoresNull` (AC-4)
- `Create_WhenDueDateIsInThePast_ThrowsDomainException` (AC-5)
- `Create_WhenDueDateIsToday_Succeeds` (AC-5)
- `Update_WhenDueDateIsInThePast_Succeeds` (AC-5)
- `ChangeStatus_FromTodoToDone_Succeeds` (AC-6)
- `ChangeStatus_FromDoneToTodo_Succeeds` (AC-6)
- `ChangeStatus_FromDoneToInProgress_ThrowsDomainException` (AC-6)
- `CanChangeStatusTo_FromDoneToInProgress_ReturnsFalse` (AC-6)
- `ChangeStatus_UpdatesUpdatedAt`
- `IsOverdue_WhenNotDoneAndDueDateHasPassed_ReturnsTrue` (AC-8)
- `IsOverdue_WhenDone_ReturnsFalse` (AC-8)
- `IsOverdue_WhenDueDateIsToday_ReturnsFalse` (AC-8)
- `User.Create_WithMixedCaseEmail_StoresLowercaseEmail` (AC-1)
- `User.Create_WhenNameIsEmpty_ThrowsDomainException` (AC-1)
- `User.Create_WhenNameIsLongerThan100Chars_ThrowsDomainException` (AC-1)

Use a `TaskItemBuilder` in `Builders/` instead of repeating constructor arguments.

## Definition of done
- [ ] All tests above written first and seen failing, then passing (Test-Driven Development)
- [ ] `dotnet build backend/TaskManager.slnx` with zero warnings; `dotnet format` clean
- [ ] Architecture tests still green
- [ ] Only files inside Scope changed
- [ ] Conventional commits: `test(domain)` → `feat(domain)` → `refactor(domain)`
- [ ] Final report: files changed, tests added, decisions taken, open questions
