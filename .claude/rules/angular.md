---
paths:
  - "frontend/**"
---

# Angular conventions

- Angular CLI 22 is a project dependency: run it with `npm run …` or `npx ng …` inside `frontend/`. Never use the global `ng` (17.3.8, a different version), never `npm install -g`, never change the Node version (22.22.0).
- Angular 22, standalone components, zoneless. Every component uses `ChangeDetectionStrategy.OnPush` and `inject()` (no constructor injection).
- State: one store per feature, a service with `signal()` / `computed()` and intention-revealing methods. Components read signals and call store methods; `*.api.ts` services only do HTTP. No NgRx.
- Templates use `@if` / `@for` / `@switch`; every `@for` has a `track` expression.
- Forms are typed reactive forms. Every field has a `<mat-label>`; validation messages mirror the backend rules in `docs/design/DD-03-frontend.md` section 5.
- Guards and interceptors are functional (`CanActivateFn`, `HttpInterceptorFn`).
- API calls use relative URLs (`/api/...`); the dev proxy and nginx handle routing. Never hard-code a host or port.
- Angular Material for UI; responsive layouts with `BreakpointObserver` (table on desktop, cards on handsets).
- No `any`; types for API payloads live in `core/models` and mirror the DTOs in `docs/api.md`.
- Never bind user data with `innerHTML`.
- Tests: Vitest specs next to the file (`*.spec.ts`), `HttpTestingController` for HTTP, test names as readable sentences.
- Feature routes are exported from the feature (`auth.routes.ts`, `tasks.routes.ts`). `app.routes.ts` and `app.config.ts` are orchestrator-owned.
- Keep the browser console clean: no errors, no warnings, no `NG0` messages.
