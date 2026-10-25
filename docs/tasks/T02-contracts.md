# T02 — Contracts: ports, DTOs, Result/Error and entity shapes

- Phase: 0 — Foundation
- Depends on: T00, T01
- Agent: orchestrator (main session)
- Design: `docs/design/DD-01-authentication.md` section 3, `docs/design/DD-02-task-management.md` sections 3 and 4

## Goal
Put every shared contract on `main` so the Phase 1–3 tracks can work in parallel against stable types.

## Context
- Spec: `docs/spec/user-story.md`
- Design: DD-01 section 3, DD-02 sections 3 and 4
- ADRs: ADR-0002, ADR-0006, ADR-0007

## Scope — files you may create or change
- `backend/src/TaskManager.Domain/`:
  - `Users/User.cs`, `Tasks/TaskItem.cs`, `Tasks/TaskItemStatus.cs`, `Common/DomainException.cs`;
  - **shapes only**: properties and method signatures from the design docs; behavior bodies `throw new NotImplementedException()` (T10 implements them).
- `backend/src/TaskManager.Application/`:
  - `Common/` (`Result`, `Result<T>`, `Error`, `ErrorType`, `PagedResult<T>`, `PagedResponse<T>`), fully implemented;
  - `Abstractions/` (`IUserRepository`, `ITaskRepository`, `IPasswordHasher`, `ITokenService`, `AccessToken`, `ICurrentUser`);
  - `Auth/` (DTOs, `IAuthService`), `Tasks/` (DTOs without `FromEntity`, `ITaskService`);
  - `DependencyInjection.cs` (empty `AddApplication()`).
- `backend/src/TaskManager.Infrastructure/DependencyInjection.cs` (empty `AddInfrastructure(IConfiguration)`)
- `backend/tests/TaskManager.Application.UnitTests/Common/**`
- `docs/api.md` (both APIs: endpoints, request and response examples, error codes)

## Out of scope
- Entity behavior (T10), service implementations (T20, T21), EF Core (T11), controllers (T30–T32)

## Tests to write first (red)
- `Success_WithValue_IsSuccessAndExposesValue`
- `Failure_WithError_IsFailureAndExposesError`
- `Value_OnFailure_ThrowsInvalidOperationException`
- `ImplicitConversion_FromError_CreatesFailure`

## Definition of done
- [ ] Tests above written first, then passing
- [ ] Every type and signature matches the design docs exactly; any deviation is written back into the design doc in the same PR
- [ ] Zero build warnings; `dotnet format` clean
- [ ] Conventional commits: `test:` → `feat:` → `docs:`
