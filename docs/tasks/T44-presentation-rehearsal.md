# T44 — Presentation rehearsal and demo recording

- Phase: 4 — Delivery
- Depends on: T43
- Agent: developer
- Design: implementation plan section 18 (presentation outline and design FAQ)

## Goal
Rehearse the 20-minute presentation and record a backup demo in case the live one fails.

## Context
- Implementation plan section 18: six blocks (spec and design, architecture, live demo, code, tests, Generative AI) and the design FAQ

## Scope — files you may create or change
- None in the repository (the recording is kept outside it)

## Checks
- [ ] Full run-through within 20 minutes, following the section 18 timings
- [ ] Live demo script works from a fresh `docker compose up`: login → list → create → edit → delete → mobile width → other user → Scalar 401 and 200
- [ ] Code walkthrough ready: `TasksController.Create` → `TaskService` → `TaskItem.Create` → repository
- [ ] Test run ready: `dotnet test` with Testcontainers starting, an architecture test, `git log` showing the TDD cycle
- [ ] Answers rehearsed for every question in the design FAQ
- [ ] Backup recording of the demo saved and playable offline
