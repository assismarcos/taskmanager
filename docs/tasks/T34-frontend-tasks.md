# T34 — Frontend tasks: list, filters, form, delete, responsive

- Phase: 3 — API
- Depends on: T24, T32
- Agent: frontend
- Design: `docs/design/DD-03-frontend.md`

## Goal
Deliver the task screens: list with status filter, pagination and overdue highlight; one form for create and edit; delete with confirmation. They work at desktop and mobile widths.

## Context
- Spec: `docs/spec/user-story.md` — AC-4, AC-5, AC-6, AC-7, AC-8
- Design: DD-03 sections 2, 3, 5 and 6; DD-02 section 7 (UTC "today")
- ADRs: ADR-0009
- Contracts: `docs/api.md` (Tasks API); `core/models` (from T12)

## Scope — files you may create or change
- `frontend/src/app/features/tasks/**` (`data-access/tasks.api.ts`, `data-access/tasks.store.ts`, pages, components, `tasks.routes.ts` exporting `TASKS_ROUTES` with `authGuard`)
- `frontend/src/app/shared/ui/**` (`status-badge`, `confirm-dialog`, `empty-state`, `loading`)

## Out of scope
- `app.routes.ts` and `app.config.ts`. The orchestrator wires `TASKS_ROUTES`; describe it in the final report.
- Auth code (T24); backend

## Tests to write first (red) — from DD-03, section 6
- `TasksStore.load sets tasks and totalCount` (AC-7)
- `TasksStore.setStatusFilter reloads from page 1` (AC-7)
- `TasksStore.remove removes the task after the API confirms`
- `TaskFormPage shows a validation message when the title is too short` (AC-4)
- `TaskFormPage blocks a past due date on create` (AC-5)
- `TaskFormPage offers only Done and Todo when editing a Done task` (AC-6)
- `TaskFormPage maps 400 field errors to form controls` (AC-4, AC-5)
- `TaskCard shows the overdue badge when isOverdue is true` (AC-8)

## Definition of done
- [ ] All tests above written first and seen failing, then passing (`npm test`)
- [ ] `npm run lint` and `npm run build` clean
- [ ] Manually checked against the running API: full CRUD at desktop width and at 375 px; loading, empty and error states; no console errors or warnings
- [ ] `track` in every `@for`; `OnPush` and `inject()` everywhere
- [ ] Only files inside Scope changed
- [ ] Conventional commits: `test(web)` → `feat(web)` → `refactor(web)`
- [ ] Final report: files changed, tests added, wiring needed, open questions
