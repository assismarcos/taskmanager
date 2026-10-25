# T01 — Spec, design docs, ADRs, task files and AI tooling

- Phase: 0 — Foundation
- Depends on: —
- Agent: developer + orchestrator (main session)
- Design: `docs/implementation-plan-en-us.md` (sections 2, 4, 12, 13, 15)

## Goal
Write the artifacts every later task depends on: the spec, the four design docs, the ADRs, one file per task, `CLAUDE.md` and the `.claude/` configuration.

## Context
- Spec decisions taken with the developer on 2026-10-25:
  - status transitions: Done goes back only to Todo;
  - `backend/TaskManager.slnx` layout;
  - Angular 22 installed only inside `frontend/` on the existing Node 22.22.0, `node:22-alpine` in Docker;
  - nothing installed, updated or reconfigured globally;
  - all 22 task files written now;
  - one `backend` agent for the four layers instead of one agent per layer (plan section 14);
  - no `new-design-doc` or `break-into-tasks` skills: the design docs and task files were written
    directly in this task (plan section 15).
- Subagents do not see the main conversation, so everything they need must be in these files.

## Scope — files you may create or change
- `docs/spec/user-story.md`
- `docs/design/DD-00-template.md`, `DD-01` to `DD-04`
- `docs/adr/0000-template.md`, `0001` to `0011`
- `docs/tasks/T*.md`
- `CLAUDE.md`, `.gitignore` (minimal), `.worktreeinclude`
- `.claude/settings.json`, `.claude/rules/*.md`, `.claude/agents/*.md`, `.claude/skills/*/SKILL.md`

## Out of scope
- Any code, solution or CI file (T00, T02)
- Edits to `docs/implementation-plan-*.md`; deviations are recorded in the design docs and ADRs

## Checks
- [ ] Every task in plan section 13 has exactly one file here, and tasks in the same phase have disjoint Scope paths
- [ ] Each acceptance criterion AC-1 to AC-9 appears in at least one design doc section 6 and in at least one task's tests
- [ ] Every `docs/...` path mentioned in these files exists or is created by a named task
- [ ] `CLAUDE.md` under 200 lines; `.claude/settings.json` is valid JSON
- [ ] In a new session, `/agents` lists 5 agents and `/` lists the 4 skills
- [ ] Developer reviewed and approved; merged to `main` before Phase 1 (agent worktrees branch from `main`)
