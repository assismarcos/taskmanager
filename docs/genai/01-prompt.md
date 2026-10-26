# 01 - The prompt

This is the prompt used to generate the Tasks API scaffold. It is structured as role, context, requirements, constraints and output format. In the real project the same content was also split into the spec, the design docs and the task files, so each agent got a smaller, focused version of it (see [05-workflow.md](05-workflow.md)).

```text
You are a senior .NET engineer. Generate a RESTful API for a task management system.

Context
- .NET 10, ASP.NET Core Web API with controllers, EF Core with PostgreSQL.
- Clean Architecture: Domain, Application, Infrastructure, Api projects.
- A User entity and JWT authentication already exist; the current user id
  comes from the `sub` claim via an ICurrentUser service.

Requirements
- TaskItem: Id (Guid), Title, Description, Status (Todo | InProgress | Done),
  DueDate, UserId, CreatedAt, UpdatedAt.
- Endpoints: GET /api/tasks (filter by status, paginated), GET /api/tasks/{id},
  POST /api/tasks, PUT /api/tasks/{id}, DELETE /api/tasks/{id}. All require auth.
- A user can only read or change their own tasks; another user's task returns 404.

Constraints
- Title required, 3-100 chars; description max 1000; due date not in the past on create.
- Return DTOs, never entities. Errors as ProblemDetails (RFC 9457).
- Correct status codes: 201 with Location on create, 204 on delete.
- Use TimeProvider, not DateTime.Now. Pass CancellationToken end to end.
- Do not use MediatR or AutoMapper.

Output
1. The files, grouped by project, with full paths.
2. xUnit tests for the service, written so they would fail without the implementation.
3. A list of assumptions you made and anything you were unsure about.
```

## Why each block is there

| Block | Purpose |
| --- | --- |
| Role | Sets the level of care and the technology family. It is short on purpose; the other blocks do the real work. |
| Context | Fixes the architecture and what already exists. Without it the model invents its own structure, its own authentication and its own way to find the current user. |
| Requirements | The entity fields, the endpoints and the ownership rule, so the model does not guess the contract. The 404 rule is stated here because it is the most important authorization rule (AC-3). |
| Constraints | Aimed at mistakes that AI-generated code makes often: returning entities, `DateTime.Now`, wrong status codes, missing `CancellationToken`, and extra libraries the project does not want (ADR-0007). |
| Output 1 | Full paths per project make it possible to see at once whether the code respects the layers. |
| Output 2 | Tests that "fail without the implementation" push the model to write tests that check behavior, not mocks. |
| Output 3 | Forces the model to list its assumptions and doubts instead of hiding them. Those lists became review questions. |

## How the prompt was used

The prompt is the starting point, not the whole process. The accepted decisions (ADRs), the acceptance criteria and the design docs replaced the "Context" and "Requirements" blocks for the agents, and the code rules in `CLAUDE.md` replaced most "Constraints". The output was never accepted as is: [03-review-log.md](03-review-log.md) lists what review found.
