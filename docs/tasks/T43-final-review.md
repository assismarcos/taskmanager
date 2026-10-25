# T43 — Final review and delivery checklist

- Phase: 4 — Delivery
- Depends on: T41, T42
- Agent: code-reviewer + developer
- Design: implementation plan section 17 (delivery checklist)

## Goal
Confirm the delivery is complete, consistent and clean before the presentation.

## Context
- Implementation plan section 17 (delivery checklist) and section 3 (requirements traceability)
- All ADRs (each must still match the code)

## Scope — files you may create or change
- None for the review itself. Fixes are small, separate commits by the orchestrator or a follow-up task, each listed in the report.

## Checks (implementation plan section 17)
- [ ] Fresh clone + `docker compose up --build` works with no extra steps
- [ ] Demo user logs in; the second user cannot see the first user's tasks
- [ ] Full CRUD in the frontend, at desktop and mobile width
- [ ] Browser console free of errors and warnings
- [ ] `dotnet build` with no warnings; all tests green, including integration; architecture tests green
- [ ] No real secrets in the repository or its history (search for "password", "key", "secret")
- [ ] Scalar shows both APIs separately, with authentication working
- [ ] All ADRs Accepted and consistent with the code
- [ ] `docs/genai/` complete, with real examples
- [ ] Readable history showing `test → feat → refactor`; agent branches merged or removed; no `WIP` on `main`
- [ ] Green CI on the last commit, with the badge in the README

## Definition of done
- [ ] `code-reviewer` report on `main` with no open Critical findings
- [ ] Every checklist item ticked or explicitly accepted by the developer
