---
paths:
  - "backend/tests/**"
---

# .NET test conventions (Test-Driven Development)

- Write the test first and watch it fail for the expected reason before writing production code (`tdd-cycle` skill).
- Name tests `Method_Scenario_ExpectedResult`, e.g. `Create_WhenDueDateIsInThePast_ThrowsDomainException`. Use the names listed in the task file.
- One behavior per test, with visible `// Arrange`, `// Act`, `// Assert` sections. Cite the acceptance criterion in a comment when the task lists one (`// AC-3`).
- Assertions with Shouldly; mocks with NSubstitute; dates with `FakeTimeProvider`. No FluentAssertions, no EF Core InMemory provider.
- Build test data with builders in the project's `Builders/` folder (`TaskItemBuilder`, `UserBuilder`) instead of repeating constructor arguments.
- Mock only ports (interfaces in `Application/Abstractions`). Assert on observable behavior (returned `Result`, saved state); verify a call only when the call is the behavior.
- Every test class that needs Docker has `[Trait("Category", "Integration")]` and joins the `"Postgres"` collection fixture. Reset data with the fixture's `ResetAsync()`; never depend on test order.
- Test projects mirror the folders of the project under test (`Application.UnitTests/Tasks/TaskServiceTests.cs`).
