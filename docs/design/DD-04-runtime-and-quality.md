# DD-04 — Runtime and quality

- Status: Approved
- Spec: `docs/spec/user-story.md` (AC-9 directly; every criterion through the test strategy)
- Related ADRs: ADR-0002, ADR-0003, ADR-0008, ADR-0011
- Plan reference: implementation plan sections 5, 9, 11 and 17

## 1. Goal and scope

Defines how the project is built, run and verified:
- the repository layout and build settings;
- one-command local execution with demo data (`docker compose up --build`);
- the test pyramid and its fixtures;
- CI on GitHub Actions;
- the delivery checks.

Out of scope: cloud deployment, observability stack, production secrets management (listed as future improvements).

## 2. Proposed solution

```text
browser ──► web (nginx :80, published 4200) ──/api──► api (ASP.NET Core :8080, published 5080) ──► db (PostgreSQL 18 :5432)
```

- **Solution layout**: everything .NET lives under `backend/`: `TaskManager.slnx` (the .NET 10 default format), `Directory.Build.props`, `Directory.Packages.props`, `src/` and `tests/`. The compose build context `./backend` therefore contains every file the restore needs (ADR-0002).
- **Build settings** (`Directory.Build.props`): `net10.0`, `Nullable` enabled, `TreatWarningsAsErrors`, .NET analyzers on, `ImplicitUsings` on. Central Package Management pins one version per package.
- **Startup in Development**: the API applies migrations (`Database.MigrateAsync()`) and runs the idempotent demo seed. Other environments do neither; the README documents `dotnet ef migrations bundle` for production.
- **Docker images**: multi-stage. Backend: `mcr.microsoft.com/dotnet/sdk:10.0` → `aspnet:10.0`, running as `USER app`. Frontend: `node:22-alpine` → `nginx:alpine`. Each project directory has a `.dockerignore` (`bin/`, `obj/`, `node_modules/`, `.angular/`).
- **CI** (`.github/workflows/ci.yml`, on push to `main` and on pull requests):
  - `backend` job: `setup-dotnet 10.0.x` → restore → build → `dotnet format --verify-no-changes` → unit tests → integration tests (the hosted runner has Docker) → coverage artifact.
  - `frontend` job (added by T12): `setup-node 22` → `npm ci` → lint → test → production build.

## 3. Contracts

Compose services (implementation plan section 11):

| Service | Image / build | Port | Health |
| --- | --- | --- | --- |
| `db` | `postgres:18-alpine`, volume `pgdata:/var/lib/postgresql` | 5432 | `pg_isready` |
| `api` | `backend/Dockerfile` | 5080 → 8080 | `GET /health` (checks the database) |
| `web` | `frontend/Dockerfile` | 4200 → 80 | — |

`depends_on`: `api` waits for `db` to be healthy; `web` waits for `api`.

Configuration (environment only, never in `appsettings.json`):

| Setting | Source | Demo value in `.env.example` |
| --- | --- | --- |
| `POSTGRES_USER`, `POSTGRES_PASSWORD` | `.env` → `db` and `api` | `taskmanager` / a demo password |
| `ConnectionStrings__Default` | compose, built from the above | `Host=db;Database=taskmanager;…` |
| `Jwt__Key` | `.env` `JWT_KEY` | demo key, at least 32 characters |
| `Jwt__Issuer`, `Jwt__Audience`, `Jwt__ExpirationMinutes` | `appsettings.json` defaults | `TaskManager`, `TaskManager.Web`, `60` |
| `Cors__AllowedOrigins__0` | `appsettings.Development.json` | `http://localhost:4200` |
| `RateLimiting__LoginPermitLimit` | `appsettings.json` default | `5` |

URLs: app `http://localhost:4200`, API `http://localhost:5080`, API reference (Scalar, both documents) `http://localhost:5080/scalar`.

## 4. Data model

Demo seed (Development only, idempotent: it does nothing if the demo user already exists):

| User | Password | Tasks |
| --- | --- | --- |
| `demo@taskmanager.local` (Demo User) | `Demo@12345` | 8 tasks, due dates relative to today: 3 `Todo` (one of them overdue), 2 `InProgress` (one overdue), 2 `Done` (one with a past due date, which is not overdue), and 1 `Todo` far in the future |
| `other@taskmanager.local` (Other User) | `Other@12345` | 2 tasks, used to show that users cannot see each other's tasks |

Overdue demo tasks are built with `TaskItem.Create(…, now: a past date)`. This respects the domain invariant (the due date was not in the past when the task was "created") and gives realistic `created_at` values.

## 5. Business rules and validation

| Rule | Where |
| --- | --- |
| Migrations and seed run at startup only in Development | `Program.cs` calls the Infrastructure database initializer |
| Secrets come only from environment variables; `.env` is git-ignored and `.env.example` is committed | compose, `.gitignore` |
| The app refuses to start without a valid JWT key | `JwtOptions` with `ValidateOnStart` |
| Containers run as non-root | `USER app` in the backend image; the nginx default user for the frontend |
| Build has zero warnings and is `dotnet format` clean | `Directory.Build.props`, CI |
| Dependencies only point inward | Architecture tests (T03) |

## 6. Testing approach

| Project | Type | Covers | Tools |
| --- | --- | --- | --- |
| `TaskManager.Domain.UnitTests` | Unit | Entity invariants, status transitions, `IsOverdue` | xUnit v3, Shouldly |
| `TaskManager.Application.UnitTests` | Unit | Use cases and validators | xUnit v3, NSubstitute, `FakeTimeProvider` |
| `TaskManager.Api.UnitTests` | Unit | `Result` → HTTP mapping, `Location` on 201 | xUnit v3, NSubstitute |
| `TaskManager.Infrastructure.IntegrationTests` | Integration | Mappings, migrations, repositories, JWT, hashing, seed | Testcontainers.PostgreSql, Respawn |
| `TaskManager.Api.IntegrationTests` | End to end | Real HTTP through the whole stack | `WebApplicationFactory`, Testcontainers |
| `TaskManager.ArchitectureTests` | Architecture | Dependency rules | NetArchTest.Rules |

Fixtures and conventions:
- `PostgresFixture : IAsyncLifetime` starts `postgres:18-alpine` once per test run, applies migrations and exposes `ResetAsync()` (Respawn). It is shared through the xUnit collection `"Postgres"`.
- `Api.IntegrationTests` has its own copy of the fixture plus a `CustomWebApplicationFactory`. The factory overrides the connection string, the JWT key, `TimeProvider` (with a `FakeTimeProvider`) and the login rate limit. `AuthenticateAsync()` registers and logs in a user and returns an authenticated `HttpClient`.
- Every test class that needs Docker has `[Trait("Category", "Integration")]`, so the unit suite runs without Docker.
- Names follow `Method_Scenario_ExpectedResult`, with visible Arrange / Act / Assert and one behavior per test. Test data builders (`TaskItemBuilder`, `UserBuilder`) live in each test project's `Builders/` folder.
- Coverage: `coverlet.collector` + ReportGenerator, aiming for 80% or more on Domain and Application. The summary goes in the README (T41).

Architecture tests (T03), all named `<Layer>_Should…`:
- `Domain_ShouldNotDependOnAnyOtherProject`
- `Domain_ShouldNotDependOnEntityFrameworkCore`
- `Application_ShouldNotDependOnInfrastructureOrApi`
- `Application_ShouldNotDependOnEntityFrameworkCoreOrAspNetCore`
- `Infrastructure_ShouldNotDependOnApi`
- `Controllers_ShouldNotDependOnInfrastructure`

Smoke test (T40): a fresh clone, `cp .env.example .env`, `docker compose up --build`. Then check that all three services are healthy, `GET /health` returns 200, the demo user can log in from the UI, and Scalar lists both documents.

## 7. Risks and open questions

- **Docker Desktop must be running** for integration tests and compose. On Windows, Testcontainers uses the Linux engine (WSL2).
- **The global toolchain is fixed.** The developer's machine keeps its installed Node.js 22.22.0, global Angular CLI 17.3.8 and .NET SDK 10.0.401. Everything the project needs is installed locally: `frontend/node_modules`, `.config/dotnet-tools.json`, and a `global.json` that pins the SDK for this repository only. `.claude/settings.json` denies global installs (`npm -g`, `nvm`, `dotnet tool -g`) and the global `ng`.
- **Test platform**: xUnit v3 can run on Microsoft.Testing.Platform, where `dotnet test` filter syntax differs from VSTest. T00 picks the platform, confirms the unit-only filter command, and updates `CLAUDE.md` if it differs from `--filter "Category!=Integration"`.
- **PostgreSQL 18 images** expect the volume at `/var/lib/postgresql`, not `/var/lib/postgresql/data`.
- The fixture is duplicated across two test projects. This is a deliberate small duplication instead of a shared test-utilities project; revisit if it grows.
- Local ports 5432, 5080 and 4200 must be free.

## 8. Task breakdown

Primary tasks (implementation plan section 13): **T00** (solution, build settings, CI), **T03** (architecture tests), **T33** (end-to-end API tests), **T40** (Dockerfiles, compose, smoke test).
Also contributing: T11 (`PostgresFixture`), T23 (seed), T41 (coverage report and README).
