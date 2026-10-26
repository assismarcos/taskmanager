# T24 — Frontend auth: login, register, interceptors, guard

- Phase: 2 — Use cases
- Depends on: T12
- Agent: frontend
- Design: `docs/design/DD-03-frontend.md`

## Goal
Let a user register, log in and log out. Protect the task routes, attach the token to API calls, and react to 401.

## Context
- Spec: `docs/spec/user-story.md` — AC-1, AC-2, AC-9
- Design: DD-03 sections 2, 3, 5 and 6
- ADRs: ADR-0009, ADR-0010
- Contracts: `docs/api.md` (Auth API); `core/models` (from T12)

## Scope — files you may create or change
- `frontend/src/app/core/auth/**` (`AuthService`, `TokenStorage`, `authGuard`, `authInterceptor`)
- `frontend/src/app/core/http/**` (`errorInterceptor`)
- `frontend/src/app/features/auth/**` (`login/login.component.*`, `register/register.component.*`, `auth.routes.ts` exporting `AUTH_ROUTES`)
- `frontend/src/app/layout/**` (user name and logout button in the toolbar)

## Out of scope
- `app.routes.ts` and `app.config.ts`. The orchestrator wires `AUTH_ROUTES` and the interceptors after the merge; describe exactly what to add in the final report.
- Task pages (T34)

## Tests to write first (red) — from DD-03, section 6
- `AuthService.login stores the session and sets currentUser` (AC-2)
- `AuthService.logout clears the session`
- `TokenStorage treats an expired session as empty`
- `authGuard redirects to /login with returnUrl when not authenticated` (AC-9)
- `authInterceptor adds the bearer token to /api requests`
- `errorInterceptor logs out and redirects on 401` (AC-9)
- `LoginComponent shows "Invalid email or password" on 401` (AC-2)
- `RegisterComponent shows a validation message when the password has no digit` (AC-1)
- `RegisterComponent shows the email error on 409` (AC-1)

## Definition of done
- [ ] All tests above written first and seen failing, then passing (`npm test`)
- [ ] `npm run lint` and `npm run build` clean; no console errors
- [ ] Forms are typed reactive forms with a `<mat-label>` on every field
- [ ] Only files inside Scope changed
- [ ] Conventional commits: `test(web)` → `feat(web)` → `refactor(web)`
- [ ] Final report: files changed, tests added, wiring needed in `app.routes.ts` / `app.config.ts`, open questions
