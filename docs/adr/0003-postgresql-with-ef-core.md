# 0003 — Use PostgreSQL with EF Core

- Status: Accepted
- Date: 2026-10-25

## Context
The brief needs a database with a users table and a data table, set up so a reviewer can run it locally with seeded data. The model is relational: each task belongs to one user. The schema must be versioned, and integration tests should run against the real engine.

## Decision
Use PostgreSQL 18 (`postgres:18-alpine`) in a container, accessed through EF Core 10 with the Npgsql provider. Mappings are written as `IEntityTypeConfiguration<T>` classes (never attributes in the Domain), names use snake_case via `EFCore.NamingConventions`, enums are stored as text, and the schema evolves through EF Core migrations. Migrations are applied automatically only in Development; production uses a migration bundle.

## Alternatives considered
- SQL Server — equally valid, but its container image is heavier and the Linux/ARM story is weaker for reviewers.
- SQLite — simpler to run, but behaves differently from a server database (types, concurrency), so tests would prove less.
- Dapper with hand-written SQL — more control, more code, and no migrations out of the box.
- MongoDB — the data is relational; a document store gives up foreign keys and the unique index for no gain.

## Consequences
- Positive: real foreign key, unique index and composite index; versioned schema; the same engine in Docker, tests (Testcontainers) and CI.
- Negative / accepted trade-offs: Docker is required to run the app and the integration tests; EF Core adds a learning surface (tracking, migrations).
