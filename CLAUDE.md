# Task Manager — project instructions

Full-stack task manager (ASP.NET Core + Angular) for a technical interview. Clarity beats cleverness:
every change must be easy to explain in a code review.

## Read before coding (spec-driven)
- `docs/spec/user-story.md` — what and why: scope, acceptance criteria AC-1 to AC-9
- `docs/design/` — the design doc (DD-xx) of your feature: how it is built
- `docs/adr/` — accepted decisions; do not contradict them without a new ADR
- The task file in `docs/tasks/` you were given — your exact scope
- `docs/implementation-plan-en-us.md` — background only; where it differs, design docs and ADRs win

## Repository layout
- `backend/` — `TaskManager.slnx`, `Directory.Build.props`, `Directory.Packages.props`,
  `src/` (Domain, Application, Infrastructure, Api) and `tests/` (six test projects)
- `frontend/` — Angular 21 app: `src/app/core`, `shared`, `layout`, `features`. One folder per component
  with `name.component.ts` (logic), `.html` (template) and `.scss` (styles, only if any); never inline them
- `docs/` — spec, design, adr, tasks, genai
- `.claude/` — `settings.json` (enforced), `rules/` (path-scoped conventions), `agents/`, `skills/`

## Commands
- Build: `dotnet build backend/TaskManager.slnx`
- Unit tests: `dotnet test backend/TaskManager.slnx --filter "Category!=Integration"`
- All tests (Docker Desktop must be running): `dotnet test backend/TaskManager.slnx`
- Format: `dotnet format backend/TaskManager.slnx` (CI uses `--verify-no-changes`)
- New migration: `dotnet ef migrations add <Name> --project backend/src/TaskManager.Infrastructure --output-dir Persistence/Migrations`
- Frontend: `cd frontend && npm test` / `npm run lint` / `npm run build`
  (Angular CLI 21 is a project dependency: use `npm run …` or `npx ng …`, never the global `ng`)
- Full stack: `docker compose up --build` — app :4200, API :5080, API reference :5080/scalar

These commands work once T00 (backend) and T12 (frontend) are merged.

## Architecture rules
- Domain depends on nothing (no NuGet packages). Application depends only on Domain.
- Infrastructure and Api depend on Application. Never reference EF Core
  or ASP.NET Core from Domain or Application.
- Expected failures return `Result<T>` with a typed `Error`; exceptions only for the unexpected.
- Never return entities from controllers; use DTOs with manual mapping.
- Every task query filters by the current user inside the repository.
  The user id comes only from `ICurrentUser` (the token), never from the request.
- Use the injected `TimeProvider`, never `DateTime.Now` or `DateTime.UtcNow`.
  Pass `CancellationToken` end to end.
- No MediatR, AutoMapper, FluentAssertions or `IUnitOfWork` (ADR-0007).

## Code comments
- Comments are the exception: names and small methods say what the code does.
- A comment says only *why*: a non-obvious decision, a workaround, a rule reference
  (`// AC-6`, `// ADR-0006`). One line, two at most.
- Never: comments that restate the code, commented-out code, banners or section dividers,
  change history or authorship (`// added for T21`), TODOs (open work goes in a task file).
- XML doc (`///`) and JSDoc only when they add something the signature does not say, in one line.
- Allowed markers: `// Arrange`, `// Act`, `// Assert` and `// AC-n` in tests; the one-line reason
  next to a suppressed warning.

## Workflow
- Spec-driven: no code without a task file; no task without a design doc.
- In this repo "TDD" always means Test-Driven Development. Technical design
  documents are called design docs (DD-xx).
- Test-Driven Development: failing test → commit `test:` → make it pass →
  commit `feat:` → refactor → commit `refactor:` (see the `tdd-cycle` skill).
- Conventional Commits with a scope: `domain`, `app`, `infra`, `api`, `web`, `arch`, `e2e`.
  Small commits. Zero build warnings.
- Touch only files listed in your task's Scope section.
- Shared files are edited only by the orchestrator (main session): `backend/TaskManager.slnx`,
  `backend/Directory.Packages.props`, both `DependencyInjection.cs`, `Program.cs` (after T30),
  `frontend/src/app/app.routes.ts`, `frontend/src/app/app.config.ts`, `CLAUDE.md`, `.claude/**`.
- If the design doc is wrong or incomplete, stop and report; do not improvise.
- Never push, never merge, never read or edit `.env`. The developer reviews and merges every branch.
- Never install, update or reconfigure anything globally: no `nvm`, no `npm install -g`, no global `ng`,
  no `dotnet tool install -g`. Use the installed Node.js 22.22.0 and .NET SDK 10; project tools stay local
  (`frontend/node_modules`, `.config/dotnet-tools.json`).

## Done means
- Tests written first and green, architecture tests green, zero warnings,
  `dotnet format` clean (or `npm run lint` clean), only Scope files changed,
  and a short report: files changed, tests added, decisions taken, open questions.
