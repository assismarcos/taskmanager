---
paths:
  - "backend/**"
---

# .NET conventions

- Target `net10.0`. Nullable is on and warnings are errors: fix warnings, never suppress them without a comment explaining why.
- File-scoped namespaces; the namespace matches the folder (`TaskManager.Application.Tasks`). One public type per file.
- `sealed` on classes not designed for inheritance; `record` for DTOs; primary constructors for dependency injection.
- `async` end to end, `Async` suffix, `CancellationToken ct` as the last parameter, passed down to EF Core.
- Time comes from the injected `TimeProvider`. Domain methods receive `now` / `today` as parameters and never read a clock.
- Domain: private setters, static factory methods (`TaskItem.Create`), invariants throw `DomainException`. No NuGet packages, no EF attributes.
- Application: services return `Result` / `Result<T>`; input is validated with FluentValidation; ports are interfaces in `Abstractions/`; mapping is manual (`TaskResponse.FromEntity`).
- Infrastructure: one `IEntityTypeConfiguration<T>` per entity; snake_case via `EFCore.NamingConventions`; `AsNoTracking()` on reads; every task query filters by `user_id` inside the repository; each write calls `SaveChangesAsync`.
- Api: controllers are thin (call one service, map the `Result`); errors are ProblemDetails (RFC 9457); every action declares `ProducesResponseType` for each status it can return.
- Package versions live only in `backend/Directory.Packages.props` (orchestrator-owned). If you need a package that is not there, stop and report.
- Logging: structured `ILogger` messages; never log passwords, tokens, keys or connection strings.
