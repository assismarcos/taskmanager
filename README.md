# Task Manager

A full-stack task manager. Users register, log in and manage their own tasks (title, description, status, due date), and see which tasks are overdue. Each user only ever sees their own tasks.

- **Backend**: ASP.NET Core (.NET 10) Web API with Clean Architecture, EF Core and PostgreSQL.
- **Frontend**: Angular 21 with Angular Material.
- **Process**: written spec first, then built with Test-Driven Development and Claude Code as the AI assistant.
- **Run**: one Docker Compose command starts everything, with demo users.

## Run it from scratch

You only need **Git** and **Docker Desktop**, and Docker Desktop must be running. The .NET SDK and Node.js are only needed to run without Docker.

```bash
git clone https://github.com/assismarcos/taskmanager.git
cd taskmanager
cp .env.example .env            # PowerShell: Copy-Item .env.example .env
docker compose up --build
```

The first run downloads the base images and packages, which takes a few minutes. Later runs start in about 25 seconds with `docker compose up`. On startup the API creates the database and the demo data.

| What | URL |
| --- | --- |
| App | http://localhost:4200 |
| API reference (Scalar, both APIs) | http://localhost:5080/scalar |
| Health check | http://localhost:5080/health |

### Demo users

| Email | Password | Data |
| --- | --- | --- |
| `demo@taskmanager.local` | `Demo@12345` | 15 tasks in all statuses, 2 of them overdue |
| `other@taskmanager.local` | `Other@12345` | 20 tasks, 2 of them overdue |

Both users have two pages of tasks (10 per page).

You can also register a new user. The password needs at least 8 characters, a letter and a digit. The values in `.env.example` are for local demos only.

### Stop and reset

| Command | Effect |
| --- | --- |
| `Ctrl+C` | Stops the containers; data is kept |
| `docker compose down` | Removes the containers; data is kept |
| `docker compose down -v` | Also deletes the database. The next start seeds the demo data again, with due dates relative to that day |

### Troubleshooting

| Symptom | Fix |
| --- | --- |
| `git clone` fails with `Filename too long` (Windows) | Clone into a short path such as `C:\src`, or use `git clone -c core.longpaths=true ...` |
| `required variable ... Copy .env.example to .env` | Run the `cp .env.example .env` step |
| `additional privileges requested: pass "--allow=fs.read=..."` (Windows) | The terminal path's letter case differs from the folder on disk (for example `projects` vs `Projects`). `cd` into the folder using the exact case, or run `COMPOSE_BAKE=false docker compose up --build` |
| Port already allocated | Free ports 4200, 5080 and 5432. A local PostgreSQL often holds 5432 |
| Cannot connect to the Docker daemon | Start Docker Desktop |

## Two-minute tour

1. Log in as the demo user. The list is ordered by due date, overdue tasks are highlighted, and you can filter by status and page through the results. Wide screens show a table and phones show cards.
2. Create a task. A due date in the past is rejected.
3. Edit a `Done` task. It can only be reopened to `Todo`.
4. Delete a task. You are asked to confirm first.
5. Log in as the other user. You only see that user's 20 tasks.
6. In Scalar, call `GET /api/tasks` without a token and get 401. Then call `POST /api/auth/login`, paste the `accessToken` in the Bearer field, and call it again to get 200.

## How the exercise requirements are met

| Requirement | Where |
| --- | --- |
| A database with a data table and a users table | PostgreSQL tables `tasks` and `users`, created by EF Core migrations ([data model](docs/architecture.md#data-model)) |
| Web API with CRUD, proper verbs, parameters and return values | `TasksController`: `GET` list and one, `POST`, `PUT`, `DELETE` ([docs/api.md](docs/api.md)) |
| A second API to create a user and log in, with authorized and anonymous endpoints | `AuthController`: `register`, `login` and `status` are anonymous, `me` needs a token. It has its own OpenAPI document |
| Data access layer | `TaskManager.Infrastructure`: EF Core repositories |
| Business logic layer, independent of data and API | `TaskManager.Domain` and `TaskManager.Application`. Architecture tests fail if they reference EF Core or ASP.NET Core |
| Unit tests for data access, business logic and API | Six test projects, listed under [Tests](#tests) |
| Responsive frontend with CRUD and structured code | Angular `features/auth` and `features/tasks`, with one signal store per feature |
| README and seeded demo credentials | This file and the [demo users](#demo-users) |
| GenAI: the prompt, the output, and how it was validated and corrected | [docs/genai/](docs/genai) |
| Clean Architecture and TDD | [docs/architecture.md](docs/architecture.md). The git history shows `test:` commits before `feat:` commits |

## User story

> **As a** busy professional, **I want to** register, log in and manage my own tasks with a title, description, status and due date, **so that** I can track what I need to do and what is overdue, without seeing anyone else's tasks.

The acceptance criteria are AC-1 to AC-9 in [docs/spec/user-story.md](docs/spec/user-story.md). Key rules:
- Another user's task returns 404.
- A due date cannot be in the past on create.
- A `Done` task can only go back to `Todo`.
- A task is overdue when it is not done and its due date has passed.

## Tech stack

| Area | Choice |
| --- | --- |
| Backend | .NET 10, ASP.NET Core controllers, EF Core 10 with Npgsql, FluentValidation, JWT bearer |
| Database | PostgreSQL 18 |
| Frontend | Angular 21 (standalone components, signals, zoneless), Angular Material |
| Tests | xUnit v3, Shouldly, NSubstitute, Testcontainers, Respawn, NetArchTest, Vitest |
| Runtime and CI | Docker Compose, nginx, GitHub Actions |

## Architecture

Clean Architecture with four .NET projects. Dependencies point inward.

| Project | Responsibility |
| --- | --- |
| `TaskManager.Domain` | Entities, status rules and invariants. No dependencies |
| `TaskManager.Application` | Use cases (`AuthService`, `TaskService`), DTOs, validators, ports. Depends on Domain only |
| `TaskManager.Infrastructure` | EF Core, repositories, migrations, JWT, password hashing, seed |
| `TaskManager.Api` | Controllers, authentication, error handling, OpenAPI, `Program.cs` |

The Angular app has four folders:
- `core`: auth, interceptors, models.
- `shared`: UI pieces.
- `layout`: the app shell.
- `features`: auth and tasks.

Diagrams and the request flow are in [docs/architecture.md](docs/architecture.md).

### Key decisions

Each decision has an ADR in [docs/adr/](docs/adr).

| Decision | Why | ADR |
| --- | --- | --- |
| Four projects, dependency rule enforced by tests | Business logic stays independent of the database and the web | [0002](docs/adr/0002-clean-architecture-four-projects.md) |
| PostgreSQL with EF Core migrations | Real constraints (unique email, foreign keys) and a reproducible schema | [0003](docs/adr/0003-postgresql-with-ef-core.md) |
| Two APIs (auth and tasks) in one host | Meets the two-API requirement without running two services | [0004](docs/adr/0004-two-apis-single-host.md) |
| JWT bearer, 60 minutes, no refresh token | Simple and stateless; refresh tokens are a listed future improvement | [0005](docs/adr/0005-jwt-bearer-authentication.md) |
| Result pattern for expected failures, mapped to ProblemDetails | Errors are explicit return values, and exceptions are kept for the unexpected | [0006](docs/adr/0006-result-pattern.md) |
| No MediatR, AutoMapper, FluentAssertions or unit of work | Less indirection for an app this size | [0007](docs/adr/0007-no-mediatr-automapper-fluentassertions-uow.md) |
| Integration tests on a real PostgreSQL (Testcontainers) | EF Core InMemory would not test indexes, foreign keys or migrations | [0008](docs/adr/0008-testing-strategy.md) |
| Angular signal stores, no NgRx | Enough state management for a few screens, without an extra library | [0009](docs/adr/0009-angular-signal-stores.md) |
| Token in `sessionStorage` | Simple for a demo. The XSS trade-off is documented, and an `httpOnly` cookie is the production choice | [0010](docs/adr/0010-browser-token-storage.md) |
| Another user's task returns 404, not 403 | The API does not reveal that the task exists | [DD-02](docs/design/DD-02-task-management.md) |
| Angular 21, not 22 | Angular 22 needs Node 22.22.3 or newer, and the development machine keeps Node 22.22.0 | [DD-03](docs/design/DD-03-frontend.md) |

## API

Two APIs run in one host. Every task endpoint needs a Bearer token and only touches the caller's own tasks.

| Method | Path | Auth | Purpose |
| --- | --- | --- | --- |
| POST | `/api/auth/register` | anonymous | Create a user |
| POST | `/api/auth/login` | anonymous (5 per minute) | Get an access token |
| GET | `/api/auth/me` | token | Current user |
| GET | `/api/auth/status` | anonymous | Status and version |
| GET | `/api/tasks` | token | List, filter by status, paginate |
| GET | `/api/tasks/{id}` | token | One task |
| POST | `/api/tasks` | token | Create |
| PUT | `/api/tasks/{id}` | token | Replace |
| DELETE | `/api/tasks/{id}` | token | Delete |

Errors use ProblemDetails (RFC 9457), with field errors on 400. The full contract, error codes and examples are in [docs/api.md](docs/api.md).

## Tests

About 200 backend tests and 65 frontend specs. CI runs all of them on every pull request (`.github/workflows/ci.yml`).

| Project | Covers |
| --- | --- |
| `Domain.UnitTests` | Entities, invariants and status transitions |
| `Application.UnitTests` | Services and validators (business logic) |
| `Infrastructure.IntegrationTests` | Repositories, migrations, constraints, JWT, hashing and seed against a real PostgreSQL (data access) |
| `Api.UnitTests` | Controllers, result mapping, exception handler, current user |
| `Api.IntegrationTests` | Every endpoint over HTTP, plus 401 without a token and 404 for another user's task |
| `ArchitectureTests` | The dependency rule between the projects |
| Frontend (Vitest) | Pages, components, stores, guard and interceptors |

```bash
# Backend unit tests (no Docker needed)
dotnet test backend/TaskManager.slnx --filter "Category!=Integration"

# All backend tests (Docker must be running: Testcontainers starts PostgreSQL)
dotnet test backend/TaskManager.slnx

# Frontend
cd frontend && npm ci && npm test
```

Other checks:
- `dotnet format backend/TaskManager.slnx --verify-no-changes`
- `npm run lint`
- `npm run build`

The build treats warnings as errors.

## Running without Docker

Prerequisites: .NET SDK 10, Node.js 22 and npm. Docker is still used for the database.

```bash
cp .env.example .env
docker compose up db                              # 1. PostgreSQL only
dotnet run --project backend/src/TaskManager.Api  # 2. API on http://localhost:5080
cd frontend && npm ci && npm start                # 3. App on http://localhost:4200
```

`npm start` proxies `/api` to `http://localhost:5080`. `dotnet run` uses the `http` profile in `backend/src/TaskManager.Api/Properties/launchSettings.json`, which sets:
- the Development environment, which migrates and seeds at startup;
- port 5080;
- the database from step 1 with the demo credentials;
- a dev-only JWT key.

Outside that profile, set `ConnectionStrings__Default` and `Jwt__Key` yourself. The API refuses to start without a valid key.

## Repository layout

```text
backend/    TaskManager.slnx, src/ (Domain, Application, Infrastructure, Api), tests/ (six projects)
frontend/   Angular app: src/app/core, shared, layout, features
docs/       spec, design docs (DD-01..04), ADRs, task files, api.md, architecture.md, genai/
.claude/    Claude Code configuration: settings, agents, rules, skills
```

## AI-assisted development

The project was built with Claude Code using a spec-driven workflow:
1. A user story with acceptance criteria.
2. Design docs and ADRs.
3. One task file per unit of work.
4. Subagents implement each task test-first.
5. A separate reviewer agent checks every change.

The GenAI deliverable is in [docs/genai/](docs/genai):

| File | Content |
| --- | --- |
| [01-prompt.md](docs/genai/01-prompt.md) | The prompt used to generate the API |
| [02-output-sample.md](docs/genai/02-output-sample.md) | A representative AI output and how it was corrected |
| [03-review-log.md](docs/genai/03-review-log.md) | Issues found in AI-generated code and their fixes |
| [04-edge-cases.md](docs/genai/04-edge-cases.md) | Edge cases, authentication and validation |
| [05-workflow.md](docs/genai/05-workflow.md) | The workflow and its guard rails |

The `.claude/` folder and `CLAUDE.md` hold the configuration:
- `settings.json` sets the permissions: no push, no global installs, no reading `.env`.
- `agents/` has five agents: backend, frontend, test-engineer, docs-writer, code-reviewer.
- `rules/` has path-scoped conventions.
- `skills/` has repeated procedures.

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
- A deployment pipeline, secrets in a vault, and EF Core migration bundles instead of migrating at startup.
