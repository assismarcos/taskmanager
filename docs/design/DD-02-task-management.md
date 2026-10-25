# DD-02 — Task management

- Status: Approved
- Spec: `docs/spec/user-story.md` (AC-3 to AC-8)
- Related ADRs: ADR-0002, ADR-0003, ADR-0006, ADR-0007
- Plan reference: implementation plan sections 5, 6, 7 and 8

## 1. Goal and scope

Delivers the **Tasks API** (`/api/tasks`, OpenAPI document `tasks-v1`): CRUD for a user's own tasks, with a status filter, pagination and the overdue flag. It also covers the `TaskItem` domain model, its persistence and the ownership rule.

Out of scope: categories, sharing, attachments, notifications, bulk operations, soft delete, partial updates (PATCH).

## 2. Proposed solution

Every flow follows the same three hops: **controller → `TaskService` → `ITaskRepository`**. The controller only translates `Result` into HTTP.

- **Create**: the validator checks the input, and the due date against today from `TimeProvider`. `TaskItem.Create(currentUser.UserId, …, now)` builds the entity, `AddAsync` saves it, and the controller returns 201 with `Location: /api/tasks/{id}`.
- **Get by id**: `GetByIdAsync(currentUser.UserId, id)`. `null` becomes `task.not_found` → 404. Another user's task is also `null`, because the repository filters by user (AC-3).
- **Update** (PUT, full replacement): validate, then load as for get by id (404 if missing). Then:
  - if `task.CanChangeStatusTo(status)` is false, return `task.invalid_status_transition` → 400;
  - otherwise call `task.Update(…)` and `task.ChangeStatus(…)`, then `UpdateAsync`, and return 200.
- **Delete**: load as for get by id (404 if missing), then `DeleteAsync` → 204. This is a hard delete.
- **List**: the validator checks page ≥ 1 and pageSize 1–50. `ListAsync(userId, status, page, pageSize)` returns items and the total count, mapped to `PagedResponse<TaskResponse>`.
- **Overdue**: `TaskResponse.FromEntity(task, today)` sets `IsOverdue = task.IsOverdue(today)`, where `today` comes from `TimeProvider` (UTC). It is computed per request and never stored.

## 3. Contracts

| Verb | Route | Body / parameters | Success | Errors |
| --- | --- | --- | --- | --- |
| GET | `/api/tasks` | query `status?`, `page` (default 1), `pageSize` (default 10, max 50) | 200 + `PagedResponse<TaskResponse>` | 400, 401 |
| GET | `/api/tasks/{id:guid}` | — | 200 + `TaskResponse` | 401, 404 |
| POST | `/api/tasks` | `CreateTaskRequest` | 201 + `Location` + `TaskResponse` | 400, 401 |
| PUT | `/api/tasks/{id:guid}` | `UpdateTaskRequest` | 200 + `TaskResponse` | 400, 401, 404 |
| DELETE | `/api/tasks/{id:guid}` | — | 204 | 401, 404 |

JSON: enums as strings (`"InProgress"`), `dueDate` as `"yyyy-MM-dd"`, timestamps as ISO 8601 UTC. `userId` and entities are never returned.

```csharp
// TaskManager.Application/Tasks
public sealed record CreateTaskRequest(string Title, string? Description, DateOnly DueDate, TaskItemStatus? Status);
public sealed record UpdateTaskRequest(string Title, string? Description, DateOnly DueDate, TaskItemStatus Status);
public sealed record TaskListQuery(TaskItemStatus? Status, int Page = 1, int PageSize = 10);
public sealed record TaskResponse(Guid Id, string Title, string? Description, TaskItemStatus Status,
    DateOnly DueDate, bool IsOverdue, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt)
{
    public static TaskResponse FromEntity(TaskItem task, DateOnly today) => /* manual mapping */;
}

public interface ITaskService
{
    Task<Result<PagedResponse<TaskResponse>>> ListAsync(TaskListQuery query, CancellationToken ct);
    Task<Result<TaskResponse>> GetByIdAsync(Guid id, CancellationToken ct);
    Task<Result<TaskResponse>> CreateAsync(CreateTaskRequest request, CancellationToken ct);
    Task<Result<TaskResponse>> UpdateAsync(Guid id, UpdateTaskRequest request, CancellationToken ct);
    Task<Result> DeleteAsync(Guid id, CancellationToken ct);
}

// TaskManager.Application/Abstractions (port)
public interface ITaskRepository
{
    Task<PagedResult<TaskItem>> ListAsync(Guid userId, TaskItemStatus? status, int page, int pageSize, CancellationToken ct);
    Task<TaskItem?> GetByIdAsync(Guid userId, Guid taskId, CancellationToken ct);
    Task AddAsync(TaskItem task, CancellationToken ct);
    Task UpdateAsync(TaskItem task, CancellationToken ct);
    Task DeleteAsync(TaskItem task, CancellationToken ct);
}

// TaskManager.Application/Common (shared by both features)
public enum ErrorType { Validation, NotFound, Conflict, Unauthorized }
public sealed record Error(ErrorType Type, string Code, string Message,
    IReadOnlyDictionary<string, string[]>? Details = null);
// Result and Result<T>: IsSuccess, Value, Error; implicit conversions from T and Error (ADR-0006)
public sealed record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount);
public sealed record PagedResponse<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);
```

Errors:

| Code | Type | HTTP |
| --- | --- | --- |
| `validation` (field errors in `Details`) | Validation | 400, `ValidationProblemDetails` |
| `task.not_found` | NotFound | 404 |
| `task.invalid_status_transition` | Validation | 400 |
| `DomainException` reaching the API (a bug: validation should have caught it) | — | 400 via the global handler |

## 4. Data model

```csharp
// TaskManager.Domain/Tasks
public enum TaskItemStatus { Todo, InProgress, Done }

public sealed class TaskItem
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Title { get; private set; }
    public string? Description { get; private set; }
    public TaskItemStatus Status { get; private set; }
    public DateOnly DueDate { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public static TaskItem Create(Guid userId, string title, string? description,
        DateOnly dueDate, TaskItemStatus status, DateTimeOffset now);
    public void Update(string title, string? description, DateOnly dueDate, DateTimeOffset now);
    public bool CanChangeStatusTo(TaskItemStatus newStatus);
    public void ChangeStatus(TaskItemStatus newStatus, DateTimeOffset now);
    public bool IsOverdue(DateOnly today);
}
```

`tasks` table, from implementation plan section 6: `id` uuid PK (Guid v7), `user_id` uuid FK → `users.id` `ON DELETE CASCADE`, `title` varchar(100), `description` varchar(1000) null, `status` varchar(20) (enum stored as text), `due_date` date, `created_at` timestamptz, `updated_at` timestamptz.

Index `ix_tasks_user_id_status_due_date` on `(user_id, status, due_date)` serves the main query: my tasks, optionally filtered by status, ordered by due date. The leading `user_id` column also serves the FK. Mapping lives in `IEntityTypeConfiguration<TaskItem>` with snake_case names (ADR-0003).

## 5. Business rules and validation

| Rule | Where | AC |
| --- | --- | --- |
| Every query filters by `user_id` inside the repository; the user id comes from `ICurrentUser` | `TaskRepository`, `TaskService` | AC-3 |
| Another user's task is reported as not found (404) | `TaskService` (`null` → `task.not_found`) | AC-3 |
| Title trimmed, required, 3–100 characters | Validator (field errors) + `TaskItem` invariant | AC-4 |
| Description trimmed, empty becomes `null`, at most 1000 characters | Validator + `TaskItem` invariant | AC-4 |
| Due date ≥ today (UTC) **on create only** | `CreateTaskRequestValidator` (with `TimeProvider`) + `TaskItem.Create` invariant (with `now`) | AC-5 |
| Status transitions: Todo ⇄ InProgress, Todo/InProgress → Done, Done → Todo only; same status is a no-op | `TaskItem.CanChangeStatusTo` / `ChangeStatus` (throws on Done → InProgress); `TaskService` returns `task.invalid_status_transition` first | AC-6 |
| Status value must be a defined enum value | Validators (`IsInEnum`) | AC-6 |
| Page ≥ 1, pageSize 1–50 | `TaskListQueryValidator` | AC-7 |
| Order by `due_date`, then `created_at`, then `id` (stable pages) | `TaskRepository.ListAsync` (`AsNoTracking`) | AC-7 |
| Overdue = status ≠ Done and `DueDate < today` | `TaskItem.IsOverdue(today)` | AC-8 |
| `UpdatedAt` changes on every update and status change | `TaskItem` | — |

Validation errors use camelCase field names (`title`, `dueDate`) so the frontend can map them to form fields.

## 6. Testing approach

| Project | Tests (red first) | AC |
| --- | --- | --- |
| Domain.UnitTests (T10) | `Create_WithValidData_SetsAllFieldsAndTimestamps`; `Create_WhenTitleIsShorterThan3Chars_ThrowsDomainException`; `Create_WhenTitleIsLongerThan100Chars_ThrowsDomainException`; `Create_WhenDescriptionIsLongerThan1000Chars_ThrowsDomainException`; `Create_WithWhitespaceDescription_StoresNull` | AC-4 |
| | `Create_WhenDueDateIsInThePast_ThrowsDomainException`; `Create_WhenDueDateIsToday_Succeeds`; `Update_WhenDueDateIsInThePast_Succeeds` | AC-5 |
| | `ChangeStatus_FromTodoToDone_Succeeds`; `ChangeStatus_FromDoneToTodo_Succeeds`; `ChangeStatus_FromDoneToInProgress_ThrowsDomainException`; `CanChangeStatusTo_FromDoneToInProgress_ReturnsFalse`; `ChangeStatus_UpdatesUpdatedAt` | AC-6 |
| | `IsOverdue_WhenNotDoneAndDueDateHasPassed_ReturnsTrue`; `IsOverdue_WhenDone_ReturnsFalse`; `IsOverdue_WhenDueDateIsToday_ReturnsFalse` | AC-8 |
| Application.UnitTests (T21) | `Create_WithValidRequest_ReturnsTaskOwnedByCurrentUser`; `GetById_WhenTaskBelongsToAnotherUser_ReturnsNotFound`; `Update_WhenTaskBelongsToAnotherUser_ReturnsNotFound`; `Delete_WhenTaskBelongsToAnotherUser_ReturnsNotFound` | AC-3 |
| | `Create_WhenTitleIsEmpty_ReturnsValidationError`; `Create_WhenDescriptionIsTooLong_ReturnsValidationError` | AC-4 |
| | `Create_WhenDueDateIsInThePast_ReturnsValidationError` (with `FakeTimeProvider`) | AC-5 |
| | `Update_FromDoneToInProgress_ReturnsInvalidStatusTransition`; `Create_WithoutStatus_DefaultsToTodo` | AC-6 |
| | `List_PassesStatusAndPagingToRepository`; `ListQueryValidator_WhenPageSizeIsAbove50_ReturnsValidationError` | AC-7 |
| | `List_MapsIsOverdueUsingTimeProvider` | AC-8 |
| Infrastructure.IntegrationTests (T11, T22) | T11: `Migrations_ApplyToEmptyDatabase`; `Tasks_WithUnknownUserId_ViolateForeignKey`. T22: `List_ReturnsOnlyTasksOfGivenUser`; `GetById_WhenTaskBelongsToAnotherUser_ReturnsNull`; `List_FiltersByStatusAndOrdersByDueDate`; `List_ReturnsTotalCountAcrossPages`; `Add_StoresStatusAsText`; `DeletingUser_CascadesToTasks` | AC-3, AC-6, AC-7 |
| Api.UnitTests (T32) | `Create_WhenSuccessful_Returns201WithLocation`; `Create_WhenValidationFails_Returns400ValidationProblem`; `GetById_WhenNotFound_Returns404`; `Update_WhenSuccessful_Returns200`; `Delete_WhenSuccessful_Returns204`; `List_ReturnsPagedResponse` | — |
| Api.IntegrationTests (T33) | `TaskEndpoints_WithoutToken_Return401` (theory, all five); `CreateThenGet_ReturnsSameTask`; `GetById_TaskOfAnotherUser_Returns404`; `Update_TaskOfAnotherUser_Returns404`; `Delete_TaskOfAnotherUser_Returns404`; `Create_WithPastDueDate_Returns400WithFieldError`; `List_FilteredByStatus_ReturnsOnlyMatchingTasks`; `List_AfterDueDatePasses_MarksTaskAsOverdue` (advances `FakeTimeProvider`) | AC-3, AC-5, AC-7, AC-8, AC-9 |

## 7. Risks and open questions

- **"Today" is UTC.** A user at UTC−5 at 21:00 local is already on the next UTC day, so a task due "today" locally is flagged overdue a few hours early. The datepicker uses the same UTC rule, so the form and the API never disagree (DD-03). Future improvement: the client sends its time zone.
- **Past due date on update** is allowed on purpose (AC-5 says "on creation"), so an overdue task can be edited without moving its date.
- **Optimistic concurrency with `xmin`** is optional and the first thing to cut. Without the client sending a version (ETag / `If-Match`), it only protects the short read-modify-write window inside one request.
- **Full replacement (PUT)**: omitting `description` clears it. This is documented in the API docs.
- `GetByIdAsync` returns a tracked entity so update and delete can save it. `ListAsync` uses `AsNoTracking`.

## 8. Task breakdown

Primary tasks (implementation plan section 13): **T10**, **T11**, **T21**, **T22**, **T32**.
Also contributing: T02 (contracts and entity shapes), T33 (end-to-end tests), T34 (frontend tasks, see DD-03).
