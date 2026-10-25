# 0008 — Test with a pyramid of unit and Testcontainers integration tests

- Status: Accepted
- Date: 2026-10-25

## Context
The brief asks for unit tests for all components, including the data access layer. Mocking EF Core tests the mock, not the data access. The work is done with Test-Driven Development, so tests must be fast to run and quick to write.

## Decision
- **Unit tests** cover Domain, Application (NSubstitute, `FakeTimeProvider`) and the controllers' HTTP mapping.
- **Data access** is tested with integration tests against a real PostgreSQL container (Testcontainers), shared per test run through an xUnit collection fixture and reset with Respawn.
- **The API** is tested end to end with `WebApplicationFactory` and the same container.
- **Architecture tests** (NetArchTest) prove the dependency rules.
- Tools: xUnit v3 and Shouldly. Frontend tests use Vitest.
- Integration test classes carry `[Trait("Category", "Integration")]` so the unit suite runs without Docker.

## Alternatives considered
- EF Core InMemory provider — fast, but not relational: no unique index, foreign keys, generated SQL or migrations.
- SQLite in-memory — closer, but a different engine with different types and SQL.
- A shared test database — slow to reset and flaky with parallel agents; Testcontainers gives each run its own database on a random port.

## Consequences
- Positive: tests prove the real behavior (unique email, cascade delete, per-user filter); parallel agents and CI each get isolated databases.
- Negative / accepted trade-offs: Docker is required for the integration suite; the first run pulls the PostgreSQL image; integration tests take seconds instead of milliseconds.
