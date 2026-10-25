# DD-03 — Frontend

- Status: Approved
- Spec: `docs/spec/user-story.md` (AC-1 to AC-9, from the user's point of view)
- Related ADRs: ADR-0009, ADR-0010
- Plan reference: implementation plan section 10

## 1. Goal and scope

An Angular 21 single-page app that consumes both APIs. It provides login and register screens, a task list with status filter and pagination, one create/edit form, and delete with confirmation. It works at desktop and mobile widths, and the browser console stays clean.

Out of scope: offline mode, i18n, theme switching, push notifications. An end-to-end Playwright test is optional and is the first thing cut if time is short. Its browser download goes to the user profile, outside the project, so it needs the developer's approval first.

## 2. Proposed solution

Layers inside the app: **component → store → api**. Components only read signals and call store methods. Stores hold state in `signal()`/`computed()`. API services only do HTTP. There is no NgRx (ADR-0009).

```text
frontend/src/app/
├── app.config.ts / app.routes.ts      # shared files, owned by the orchestrator
├── core/auth/                          # AuthService, TokenStorage, authGuard, authInterceptor
├── core/http/                          # errorInterceptor
├── core/models/                        # types mirroring the API DTOs
├── shared/ui/                          # status-badge, confirm-dialog, empty-state, loading
├── layout/                             # shell: toolbar (user name + logout) + router-outlet
└── features/
    ├── auth/   login.page.ts, register.page.ts, auth.routes.ts
    └── tasks/  data-access/{tasks.api.ts, tasks.store.ts}, task-list.page.ts,
                task-form.page.ts, components/{task-card, task-filters}, tasks.routes.ts
```

Routes, all lazy loaded:

| Path | Page | Guard |
| --- | --- | --- |
| `/login`, `/register` | `LoginPage`, `RegisterPage` (`AUTH_ROUTES`) | — |
| `/tasks` | `TaskListPage` (`TASKS_ROUTES`) | `authGuard` |
| `/tasks/new`, `/tasks/:id/edit` | `TaskFormPage` (`TASKS_ROUTES`) | `authGuard` |
| `''`, `**` | redirect to `/tasks` | — |

Each feature exports its route array (`auth.routes.ts`, `tasks.routes.ts`). The orchestrator wires them into `app.routes.ts` with `loadChildren`, and registers the interceptors in `app.config.ts`, after each merge. This way no two tasks edit the shared files.

API calls use relative URLs (`/api/...`). During development `ng serve` proxies them through `proxy.conf.json` to `http://localhost:5080`; in Docker nginx proxies them to `api:8080`. The browser never makes a cross-origin call.

## 3. Contracts

```ts
// core/models
export type TaskStatus = 'Todo' | 'InProgress' | 'Done';
export interface TaskItem {
  id: string; title: string; description: string | null; status: TaskStatus;
  dueDate: string; isOverdue: boolean; createdAt: string; updatedAt: string;
}
export interface PagedResponse<T> { items: T[]; page: number; pageSize: number; totalCount: number; }
export interface User { id: string; name: string; email: string; }
export interface LoginResponse { accessToken: string; expiresAt: string; user: User; }
export interface ProblemDetails { title?: string; status?: number; detail?: string; errors?: Record<string, string[]>; }
```

| Service | Public surface |
| --- | --- |
| `AuthService` | `currentUser: Signal<User \| null>`, `isAuthenticated: Signal<boolean>`, `login()`, `register()`, `logout()`, `accessToken()` |
| `TokenStorage` | `read()`, `write(session)`, `clear()` over `sessionStorage` key `tm.session` (`{ accessToken, expiresAt, user }`); an expired session reads as empty |
| `authGuard` (`CanActivateFn`) | Not authenticated → `UrlTree` to `/login?returnUrl=…` |
| `authInterceptor` (`HttpInterceptorFn`) | Adds `Authorization: Bearer …` to `/api/` requests when a token exists |
| `errorInterceptor` (`HttpInterceptorFn`) | 401 from any endpoint except login → `logout()` + navigate to `/login`. 0/5xx → snackbar "Something went wrong". Other errors pass through to the page |
| `TasksApi` | `list(query)`, `get(id)`, `create(body)`, `update(id, body)`, `remove(id)`; HTTP only |
| `TasksStore` | Signals `tasks`, `totalCount`, `filter` (`{ status, page, pageSize }`), `loading`, `error`; computed `isEmpty`; methods `load()`, `setStatusFilter()`, `setPage()`, `create()`, `update()`, `remove()` |

## 4. Data model

| State | Where it lives | Lifetime |
| --- | --- | --- |
| Session (token, expiry, user) | `sessionStorage` via `TokenStorage`, mirrored in `AuthService` signals | Until the tab closes, logout or expiry |
| Task page, filter, loading, error | `TasksStore` signals (provided in the tasks routes) | While the tasks feature is active |
| Form values | Typed reactive forms (`FormGroup<{ title: FormControl<string> … }>`) | The page |

## 5. Business rules and validation

The backend stays the source of truth. The forms mirror its rules so the user gets feedback before submitting, and server errors are always shown too.

| Field / behavior | Rule in the UI | AC |
| --- | --- | --- |
| Name | required, max 100 | AC-1 |
| Email | required, email format, max 254 | AC-1 |
| Password | required, min 8, at least one letter and one digit | AC-1 |
| Register 409 | error shown on the email field: "Email is already registered" | AC-1 |
| Login 401 | form-level error "Invalid email or password" | AC-2 |
| Title | required, 3–100 (trimmed) | AC-4 |
| Description | max 1000, with a character counter | AC-4 |
| Due date | required; on create the datepicker's `min` is today's **UTC** date, the same rule as the API (DD-02 section 7) | AC-5 |
| Status | create: any, default `Todo`. Editing a `Done` task offers only `Done` and `Todo` | AC-6 |
| 400 with `errors` | each entry mapped to the matching form control; unknown keys shown as a form-level error | AC-4, AC-5 |
| Filter and pages | status select (All/Todo/InProgress/Done), `mat-paginator` (10/20/50); changing the filter goes back to page 1 | AC-7 |
| Overdue | badge and highlighted row/card when `isOverdue` is true; the UI never recomputes it | AC-8 |
| Protected routes | `authGuard`; any 401 sends the user to login | AC-9 |

UX and accessibility: list uses a `mat-table` on desktop and cards on handsets (`BreakpointObserver`). Every list has loading, empty and error (with retry) states. A snackbar confirms create, update and delete, and delete asks for confirmation in a dialog. Every field has a `<mat-label>`, and focus moves to the first invalid field on submit.

Clean console: `track` in every `@for`, `OnPush` and `inject()` everywhere, production build in Docker, favicon present, no 404/CORS requests, no `NG0` errors.

## 6. Testing approach

Vitest (the Angular CLI default) with `TestBed` and `HttpTestingController`. Spec files live next to the code (`*.spec.ts`).

| Task | Tests (red first) | AC |
| --- | --- | --- |
| T12 | `ShellComponent renders the toolbar and router outlet`; `app routes redirect unknown paths to /tasks` | — |
| T24 | `AuthService.login stores the session and sets currentUser`; `AuthService.logout clears the session`; `TokenStorage treats an expired session as empty` | AC-2 |
| | `authGuard redirects to /login with returnUrl when not authenticated`; `authInterceptor adds the bearer token to /api requests`; `errorInterceptor logs out and redirects on 401` | AC-9 |
| | `LoginPage shows "Invalid email or password" on 401` | AC-2 |
| | `RegisterPage shows a validation message when the password has no digit`; `RegisterPage shows the email error on 409` | AC-1 |
| T34 | `TasksStore.load sets tasks and totalCount`; `TasksStore.setStatusFilter reloads from page 1` | AC-7 |
| | `TasksStore.remove removes the task after the API confirms` | — |
| | `TaskFormPage shows a validation message when the title is too short` | AC-4 |
| | `TaskFormPage blocks a past due date on create` | AC-5 |
| | `TaskFormPage offers only Done and Todo when editing a Done task` | AC-6 |
| | `TaskCard shows the overdue badge when isOverdue is true` | AC-8 |
| | `TaskFormPage maps 400 field errors to form controls` | AC-4, AC-5 |

## 7. Risks and open questions

- **Angular 21, not 22.** Every Angular 22 release (22.0.0 to 22.2.4) requires Node `^22.22.3 || ^24.15.0 || >=26`, and the machine's Node 22.22.0 must not change. Angular 21 (21.2.x, still receiving patches) requires Node `^20.19.0 || ^22.12.0 || >=24`, and keeps everything this design relies on: standalone components, signals, zoneless and Vitest as the default test runner. Upgrade path once Node is upgraded: `npx ng update @angular/core@22 @angular/cli@22`. Docker uses `node:22-alpine`.
- **The global environment must not change.** The machine keeps Node 22.22.0 and a global Angular CLI 17.3.8. Angular 21 lives only in `frontend/node_modules`: the workspace is created with `npx --yes @angular/cli@21 new …` and every later command runs through `npm run` or `npx ng`. `NG_CLI_ANALYTICS=false` prevents the analytics prompt from writing to the global Angular config. `.claude/settings.json` denies global installs and the global `ng`.
- The UTC "today" can block picking today's local date late in the evening west of UTC. This is accepted and documented, and the API behaves the same way.
- The session is lost when the tab closes (`sessionStorage`). Accepted in ADR-0010.

## 8. Task breakdown

Primary tasks (implementation plan section 13): **T12** (app shell and base), **T24** (auth), **T34** (tasks).
The orchestrator wires `app.routes.ts` and `app.config.ts` after each merge.
