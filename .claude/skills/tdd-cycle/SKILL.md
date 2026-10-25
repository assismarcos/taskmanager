---
name: tdd-cycle
description: Test-Driven Development cycle used in this repository — red, green, refactor, with one Conventional Commit per step. Use whenever implementing a task from docs/tasks/ or writing production code that needs tests.
---

# Test-Driven Development cycle

Work through the task's "Tests to write first" list in order. For each test (or a small group of tests
for the same rule, such as a theory or the boundaries of one validation):

## 1. Red
- Write only the test. Use the exact name from the task file (`Method_Scenario_ExpectedResult`; Vitest
  uses a readable sentence). Arrange / Act / Assert, one behavior, `// AC-n` when the task cites one.
- If the production member does not exist yet, add the smallest signature that compiles and throws
  `NotImplementedException` (or returns a wrong value), so the test fails on an assertion, not on a
  compile error.
- Run the single test and confirm it fails **for the expected reason**.
- Commit: `test(<scope>): <behavior in business language>`.

## 2. Green
- Write the minimum production code that makes the test pass. No extra features.
- Run the whole test project; everything must be green.
- Commit: `feat(<scope>): <what now works>`.

## 3. Refactor
- Improve names, remove duplication, extract helpers — tests stay green after every change.
- Commit `refactor(<scope>): <what changed>` only if something changed.

## Scopes and commands

| Scope | Test command |
| --- | --- |
| `domain` | `dotnet test backend/tests/TaskManager.Domain.UnitTests` |
| `app` | `dotnet test backend/tests/TaskManager.Application.UnitTests` |
| `infra` | `dotnet test backend/tests/TaskManager.Infrastructure.IntegrationTests` (Docker) |
| `api` | `dotnet test backend/tests/TaskManager.Api.UnitTests` |
| `e2e` | `dotnet test backend/tests/TaskManager.Api.IntegrationTests` (Docker) |
| `arch` | `dotnet test backend/tests/TaskManager.ArchitectureTests` |
| `web` | `cd frontend && npm test` |

## Never
- Write production code before a failing test asks for it.
- Change a test's assertion to make it pass without stating it in the final report.
- Delete or skip a failing test to get to green.
- Commit `feat:` or `refactor:` with any failing test.
