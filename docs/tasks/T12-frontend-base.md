# T12 — Frontend base: Angular app, Material, shell, proxy, models

- Phase: 1 — Core
- Depends on: T02
- Agent: frontend
- Design: `docs/design/DD-03-frontend.md`

## Goal
Create the Angular 22 workspace with Material, linting, the shell layout, the API models and the dev proxy, ready for the auth and tasks features.

## Context
- Design: DD-03 sections 2 (structure and routes), 3 (models), 6 (tests), 7 (versions)
- ADRs: ADR-0009, ADR-0010
- Contracts: `docs/api.md` (from T02)
- Node: use the installed 22.22.0 as is (Angular 22 requires `^22.22 || ^24.13.1 || >=26`). Never run `nvm`.
- **Global environment must stay untouched** (DD-03 section 7). The machine's global Angular CLI 17.3.8 is never used or changed. Create the workspace from the repo root with the project-local CLI and no prompts:
  `NG_CLI_ANALYTICS=false npx --yes @angular/cli@22 new frontend --skip-git --style=scss --ssr=false --interactive=false`
  After that, run every command inside `frontend/` through `npm run …` or `npx ng …`. Examples: `npx ng add @angular/material --skip-confirmation`, `npx ng add angular-eslint --skip-confirmation`.

## Scope — files you may create or change
- `frontend/**` **except** `src/app/core/auth/**`, `src/app/core/http/**`, `src/app/features/**`, `src/app/shared/ui/**`
- Includes `app.config.ts`, `app.routes.ts` (first version), `proxy.conf.json` (`/api` → `http://localhost:5080`), `core/models/**`, `layout/**`, theme, ESLint and Prettier config
- `.github/workflows/ci.yml` — add the `frontend` job only

## Out of scope
- Login, register, guard, interceptors (T24); task pages (T34); Dockerfile and nginx (T40)

## Tests to write first (red) — from DD-03, section 6
- `ShellComponent renders the toolbar and router outlet`
- `app routes redirect unknown paths to /tasks`

## Definition of done
- [ ] Tests above written first, then passing (`npm test`)
- [ ] `npm run lint` and `npm run build` clean; `ng serve` shows the shell with no console errors
- [ ] Standalone, zoneless, `OnPush`, `inject()`, per `.claude/rules/angular.md`
- [ ] Global environment unchanged, compared with a snapshot taken before starting: `node --version` (22.22.0), `ng version` outside the repo (CLI 17.3.8), `npm ls -g --depth=0`
- [ ] Only files inside Scope changed
- [ ] Conventional commits: `test(web)` → `feat(web)` → `refactor(web)`
- [ ] Final report: files changed, tests added, versions used, open questions
