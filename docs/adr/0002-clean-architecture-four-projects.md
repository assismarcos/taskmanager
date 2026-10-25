# 0002 — Use Clean Architecture with four projects

- Status: Accepted
- Date: 2026-10-25

## Context
The brief asks for a data access layer and a business logic layer that is independent of data and API. The domain is small: two entities and nine acceptance criteria. Parallel agents need clear boundaries so they do not edit the same files.

## Decision
Use four projects under `backend/src/`: `TaskManager.Domain`, `TaskManager.Application`, `TaskManager.Infrastructure` and `TaskManager.Api`. Dependencies point inward: Domain depends on nothing, Application only on Domain, and Infrastructure and Api on Application. Api is the composition root. The data access layer is a set of **specific** repositories (`ITaskRepository`, `IUserRepository`) declared in Application and implemented with EF Core in Infrastructure.

All .NET files live under `backend/`: `TaskManager.slnx` (the .NET 10 default solution format), `Directory.Build.props`, `Directory.Packages.props`, `src/` and `tests/`. This keeps the Docker build context (`./backend`) self-contained.

## Alternatives considered
- Single project with folders — less ceremony, but nothing stops a controller from using `DbContext`, and the brief's layering cannot be proven.
- Vertical slices — great for large feature counts; with two features it adds a pattern without a payoff.
- Generic repository (`IRepository<T>`) — hides intent, leaks `IQueryable`, and makes the per-user filter easy to forget.
- Solution file at the repository root — the props files would sit outside the Docker build context.

## Consequences
- Positive: business rules are testable without a database; the layering is enforced by architecture tests (T03); each task stays inside one layer, so parallel agents do not edit the same files.
- Negative / accepted trade-offs: more projects and references than the size strictly needs; a request crosses three hops (controller → service → repository).
