# T21 — TaskService and validators (Application)

- Phase: 2 — Use cases
- Depends on: T10
- Agent: backend
- Design: `docs/design/DD-02-task-management.md`

## Goal
Implement CRUD use cases for tasks, enforcing ownership, validation, status transitions and the overdue mapping.

## Context
- Spec: `docs/spec/user-story.md` — AC-3, AC-4, AC-5, AC-6, AC-7, AC-8
- Design: DD-02 sections 2, 3, 5 and 6
- ADRs: ADR-0002, ADR-0006, ADR-0007
- Contracts: `ITaskService`, DTOs, `ITaskRepository`, `ICurrentUser` (on `main`)
- Validators check the **trimmed** title and description lengths, as `TaskItem` does. Otherwise a title such as `" ab "` passes validation and the domain throws, and a `DomainException` reaching the API is a bug (DD-02 section 3).

## Scope — files you may create or change
- `backend/src/TaskManager.Application/Tasks/**` (including `TaskResponse.FromEntity`)
- `backend/tests/TaskManager.Application.UnitTests/Tasks/**`
- `backend/tests/TaskManager.Application.UnitTests/Builders/TaskItemBuilder.cs` (T20 owns `UserBuilder.cs` in the same folder)

## Out of scope
- Controllers, EF Core, anything in Infrastructure or Api, `DependencyInjection.cs` (orchestrator)

## Tests to write first (red) — from DD-02, section 6
- `Create_WithValidRequest_ReturnsTaskOwnedByCurrentUser` (AC-3)
- `GetById_WhenTaskBelongsToAnotherUser_ReturnsNotFound` (AC-3)
- `Update_WhenTaskBelongsToAnotherUser_ReturnsNotFound` (AC-3)
- `Delete_WhenTaskBelongsToAnotherUser_ReturnsNotFound` (AC-3)
- `Create_WhenTitleIsEmpty_ReturnsValidationError` (AC-4)
- `Create_WhenDescriptionIsTooLong_ReturnsValidationError` (AC-4)
- `Create_WhenDueDateIsInThePast_ReturnsValidationError` (AC-5)
- `Update_FromDoneToInProgress_ReturnsInvalidStatusTransition` (AC-6)
- `Create_WithoutStatus_DefaultsToTodo` (AC-6)
- `List_PassesStatusAndPagingToRepository` (AC-7)
- `ListQueryValidator_WhenPageSizeIsAbove50_ReturnsValidationError` (AC-7)
- `List_MapsIsOverdueUsingTimeProvider` (AC-8)

Mock ports with NSubstitute; use `FakeTimeProvider`. A `TaskItemBuilder` in `Builders/` creates tasks owned by a given user.

## Definition of done
- [ ] All tests above written first and seen failing, then passing (Test-Driven Development)
- [ ] `dotnet build backend/TaskManager.slnx` with zero warnings; `dotnet format` clean
- [ ] Architecture tests still green
- [ ] Only files inside Scope changed
- [ ] Conventional commits: `test(app)` → `feat(app)` → `refactor(app)`
- [ ] PR description lists what was done and any decision taken
