# T41 — README, architecture overview and coverage report

- Phase: 4 — Delivery
- Depends on: T40
- Agent: docs-writer
- Design: implementation plan section 17 (README structure)

## Goal
A reviewer can clone, run, log in and understand the design from the README alone, with links into `docs/` for depth.

## Context
- Implementation plan section 17 (README structure, 11 sections) and section 3 (traceability table)
- `docs/spec/`, `docs/design/`, `docs/adr/`, `docs/api.md`
- Coverage: `dotnet test --collect:"XPlat Code Coverage"` + `reportgenerator` (local tool from T00)

## Scope — files you may create or change
- `README.md`
- `docs/architecture.md` (layer diagram in Mermaid, one line per project, request flow, links to ADRs)
- `docs/api.md` (bring up to date with the implemented API)

## Out of scope
- Code and configuration. If the docs reveal a mismatch with the code, report it instead of editing code.

## Checks
- [ ] README follows the 11 sections of plan section 17, with a screenshot of the task list and the CI badge
- [ ] Every command in the README was run and works as written (Windows and Linux shells noted where they differ)
- [ ] Demo credentials highlighted; coverage summary (Domain and Application ≥ 80%) included
- [ ] All links resolve; no secrets
- [ ] Conventional commits: `docs: ...`
