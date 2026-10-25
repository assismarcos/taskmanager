# 0007 — Do not use MediatR, AutoMapper, FluentAssertions or an IUnitOfWork abstraction

- Status: Accepted
- Date: 2026-10-25

## Context
These are common in .NET templates. MediatR, AutoMapper and FluentAssertions (v8+) moved to commercial licensing for business use in 2025; their status is re-checked at implementation time. Each also adds indirection. The project has two use-case services and a handful of DTOs.

## Decision
- Use explicit application services (`AuthService`, `TaskService`) called directly by controllers.
- Map manually with static factories (`TaskResponse.FromEntity`).
- Assert with Shouldly (BSD license).
- Treat EF Core's `DbContext` as the unit of work: each repository write calls `SaveChangesAsync`, and each use case changes a single aggregate, so no `IUnitOfWork` interface is needed.

## Alternatives considered
- MediatR (CQRS-style handlers, pipeline behaviors) — useful for many cross-cutting behaviors; here it hides a three-hop flow behind a dispatcher.
- AutoMapper / Mapster — saves little with so few DTOs and makes mapping errors appear at runtime instead of compile time.
- FluentAssertions v7 (last free version) — pins the project to an unmaintained major version.
- `IUnitOfWork` + generic repositories — duplicates what `DbContext` already does.

## Consequences
- Positive: a reviewer can follow `TasksController.Create` → `TaskService` → `TaskItem.Create` → repository without tooling; no license risk; fewer packages.
- Negative / accepted trade-offs: cross-cutting concerns (validation, logging) are called explicitly in services; if a use case ever changes two aggregates, a transaction must be opened explicitly.
