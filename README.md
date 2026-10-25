# Task Manager

A full-stack task manager. Users register, log in and manage their own tasks (title, description, status, due date). The backend is ASP.NET Core (.NET 10) with PostgreSQL, the frontend is Angular, and everything runs with one Docker Compose command.

CI runs on every pull request (`.github/workflows/ci.yml`): build, format check, unit and integration tests, and the frontend lint, test and build.

## Quick start

Prerequisite: Docker (Docker Desktop on Windows or macOS).

```bash
cp .env.example .env
docker compose up --build
```

On Windows PowerShell use `Copy-Item .env.example .env`.

| What | URL |
| --- | --- |
| App (Angular behind nginx) | http://localhost:4200 |
| API | http://localhost:5080 |
| API reference (Scalar, both APIs) | http://localhost:5080/scalar |

Ports 4200, 5080 and 5432 must be free. The API applies the migrations and the demo seed at startup in the Development environment, which compose uses.

### Demo users

| User | Email | Password |
| --- | --- | --- |
| Demo User (8 tasks, some overdue) | `demo@taskmanager.local` | `Demo@12345` |
| Other User (2 tasks) | `other@taskmanager.local` | `Other@12345` |

Log in as both to see that each user only sees their own tasks. The values in `.env.example` are demo values only.

To call the API from Scalar: send `POST /api/auth/login`, copy `accessToken`, and paste it in the Bearer authentication field.

## Running locally without Docker

Prerequisites: .NET SDK 10, Node.js 22 and npm, and Docker for the database.

```bash
# 1. PostgreSQL only
docker compose up db

# 2. API on http://localhost:5080
dotnet run --project backend/src/TaskManager.Api

# 3. Frontend on http://localhost:4200
cd frontend
npm ci
npm start
```

`npm start` proxies `/api` to `http://localhost:5080` (`frontend/proxy.conf.json`). `dotnet run` uses the `http` profile in `backend/src/TaskManager.Api/Properties/launchSettings.json`: Development environment, port 5080, the database from step 1 with the demo credentials from `.env.example`, and a dev-only JWT key. In Development the API applies the migrations and seeds the demo users at startup. Outside that profile, set `ConnectionStrings__Default` and `Jwt__Key` yourself; the API refuses to start without a valid JWT key.

## Tests

```bash
# Unit tests (no Docker needed)
dotnet test backend/TaskManager.slnx --filter "Category!=Integration"

# All backend tests (integration tests start PostgreSQL with Testcontainers, so Docker must be running)
dotnet test backend/TaskManager.slnx

# Frontend
cd frontend && npm test
```

Other checks: `dotnet format backend/TaskManager.slnx --verify-no-changes`, `npm run lint` and `npm run build`. Test counts and coverage are published by CI; see the pipeline run. The coverage goal is 80% or more on Domain and Application ([DD-04](docs/design/DD-04-runtime-and-quality.md)).

## Spec and design

| Document | Content |
| --- | --- |
| [docs/spec/user-story.md](docs/spec/user-story.md) | The user story and acceptance criteria AC-1 to AC-9 |
| [docs/design/](docs/design) | DD-01 authentication, DD-02 task management, DD-03 frontend, DD-04 runtime and quality |
| [docs/tasks/](docs/tasks) | One task file per unit of work |

## Architecture

Clean Architecture with four .NET projects. Dependencies point inward.

| Project | Responsibility |
| --- | --- |
| `TaskManager.Domain` | Entities, status rules and invariants. No dependencies. |
| `TaskManager.Application` | Use cases (`AuthService`, `TaskService`), DTOs, validators, ports. Depends on Domain only. |
| `TaskManager.Infrastructure` | EF Core, repositories, migrations, JWT, password hashing, seed. |
| `TaskManager.Api` | Controllers, authentication, error handling, OpenAPI, `Program.cs`. |

The Angular app has `core` (auth, interceptors, models), `shared`, `layout` and `features` (auth, tasks), with signal-based stores.

Details, diagrams and the request flow are in [docs/architecture.md](docs/architecture.md). The decisions are recorded in [docs/adr/](docs/adr) (ADR-0001 to ADR-0011).

## API

Two APIs run in one host. All task endpoints need a Bearer token and only touch the caller's own tasks.

| Method | Path | Auth | Purpose |
| --- | --- | --- | --- |
| POST | `/api/auth/register` | public | Create a user |
| POST | `/api/auth/login` | public (5 per minute) | Get an access token |
| GET | `/api/auth/me` | token | Current user |
| GET | `/api/auth/status` | public | Status and version |
| GET | `/api/tasks` | token | List, filter by status, paginate |
| GET | `/api/tasks/{id}` | token | One task |
| POST | `/api/tasks` | token | Create |
| PUT | `/api/tasks/{id}` | token | Replace |
| DELETE | `/api/tasks/{id}` | token | Delete |

Full contract, error catalog and examples: [docs/api.md](docs/api.md).

## AI-assisted development

The project was built with Claude Code using a spec-driven workflow. [docs/genai/](docs/genai) records the prompt, a first AI output with its later fix, the review log, edge cases and the workflow.

The `.claude/` folder holds the configuration: `settings.json` (permissions: no push, no global installs, no reading `.env`), `agents/` (backend, frontend, test-engineer, docs-writer, code-reviewer), `rules/` (path-scoped conventions) and `skills/` (repeated procedures). `CLAUDE.md` holds the working rules.

## Decisions and versions

- Angular 21, not 22. Every Angular 22 release needs Node 22.22.3 or newer, and the development machine keeps Node 22.22.0. See [ADR-0009](docs/adr/0009-angular-signal-stores.md) and [DD-03](docs/design/DD-03-frontend.md).
- Backend: .NET 10, EF Core 10, PostgreSQL 18. NuGet sources are limited to nuget.org by the repository `nuget.config`.

## Requirements traceability

| Criterion | Summary | Design | Tasks |
| --- | --- | --- | --- |
| AC-1 | Register, duplicate email rejected (409) | DD-01, DD-03 | T10, T11, T20, T22, T24, T31, T33 |
| AC-2 | Login, generic 401 on wrong credentials | DD-01, DD-03 | T20, T23, T24, T31, T33 |
| AC-3 | Own tasks only, others return 404 | DD-02 | T21, T22, T33 |
| AC-4 | Title 3 to 100, description up to 1000 | DD-02, DD-03 | T10, T21, T34 |
| AC-5 | Due date not in the past on create | DD-02, DD-03 | T10, T21, T33, T34 |
| AC-6 | Status transitions; `Done` only back to `Todo` | DD-02, DD-03 | T10, T21, T22, T34 |
| AC-7 | Filter by status, pagination, order by due date | DD-02, DD-03 | T21, T22, T33, T34 |
| AC-8 | Overdue flag | DD-02, DD-03 | T10, T21, T33, T34 |
| AC-9 | No token, 401 on every task endpoint | DD-01, DD-03, DD-04 | T24, T30, T33 |

## Future improvements

- Refresh tokens and an `httpOnly` cookie instead of `sessionStorage` ([ADR-0010](docs/adr/0010-browser-token-storage.md)).
- Categories, task sharing, attachments.
- Notifications for tasks that become overdue.
- Observability: structured logs, metrics and traces (OpenTelemetry).
- Deployment pipeline, secrets in a vault, and EF Core migration bundles (`dotnet ef migrations bundle`) instead of migrating at startup.
