# T40 — Dockerfiles, full compose and smoke test

- Phase: 4 — Delivery
- Depends on: T33, T34
- Agent: orchestrator (main session)
- Design: `docs/design/DD-04-runtime-and-quality.md`

## Goal
`docs/design/DD-04-runtime-and-quality.md` promises a one-command run: `docker compose up --build` starts the database, API and frontend with demo data. This task delivers it.

## Context
- Design: DD-04 sections 2, 3, 4, 5 and 7; implementation plan section 11 (compose outline)
- ADRs: ADR-0003

## Scope — files you may create or change
- `docker-compose.yml`, `.env.example`
- `backend/Dockerfile`, `backend/.dockerignore`
- `frontend/Dockerfile`, `frontend/.dockerignore`, `frontend/nginx.conf`

## Out of scope
- Application code. If the smoke test fails because of a bug, open a fix in the owning task's area and report it.

## Checks (smoke test from DD-04, section 6)
- [ ] Fresh clone in another folder → `cp .env.example .env` → `docker compose up --build` works with no extra steps
- [ ] `db` and `api` report healthy; `GET http://localhost:5080/health` → 200
- [ ] `http://localhost:4200` loads; demo user logs in and sees 8 tasks, including overdue ones
- [ ] `other@taskmanager.local` sees only their own 2 tasks
- [ ] `http://localhost:5080/scalar` shows `auth-v1` and `tasks-v1`
- [ ] Deep link refresh (e.g. `/tasks/new`) works through nginx `try_files`
- [ ] Images: multi-stage, restore layer cached, backend runs as `USER app`, frontend uses `node:22-alpine` → `nginx:alpine`
- [ ] No secret in any image or committed file; `.env` stays git-ignored

## Definition of done
- [ ] All checks above pass
- [ ] Conventional commits: `build:` / `chore:`
- [ ] Final report: image sizes, startup time, anything that needed a workaround
