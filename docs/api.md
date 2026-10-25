# Task Manager API

Two APIs run in one ASP.NET Core host, each with its own OpenAPI document:

| API | Base path | OpenAPI document | Design doc |
| --- | --- | --- | --- |
| Auth | `/api/auth` | `auth-v1` | `docs/design/DD-01-authentication.md` |
| Tasks | `/api/tasks` | `tasks-v1` | `docs/design/DD-02-task-management.md` |

Locally the API listens on `http://localhost:5080`. The interactive reference (Scalar) is at `/scalar`, and the raw documents are at `/openapi/auth-v1.json` and `/openapi/tasks-v1.json`.

## Conventions

- JSON with camelCase property names.
- Enums are strings: `"Todo"`, `"InProgress"`, `"Done"`.
- `dueDate` is a calendar date, `"yyyy-MM-dd"`. Timestamps are ISO 8601 with a UTC offset, such as `"2026-10-25T14:30:00+00:00"`.
- Protected endpoints need `Authorization: Bearer <accessToken>`, using the token from login. It is valid for 60 minutes; there is no refresh token, so the client logs in again.
- Errors are ProblemDetails (RFC 9457, `application/problem+json`). Validation errors add `errors`, keyed by camelCase field name. See [Errors](#errors).
- Messages in the examples are illustrative. Field names, status codes and response shapes are the contract.

## Auth API

### `POST /api/auth/register`

Public. Creates a user.

```json
{ "name": "Ann Lee", "email": "Ann@Mail.com", "password": "secret123" }
```

Rules: name required, at most 100 characters; email valid, at most 254 characters, stored lowercase; password at least 8 characters with a letter and a digit.

**201 Created**, with `Location: /api/auth/me`:

```json
{ "id": "0193a5b2-7c1e-7d3a-9b2f-3c4d5e6f7a8b", "name": "Ann Lee", "email": "ann@mail.com" }
```

Errors: **400** invalid input, **409** email already registered.

### `POST /api/auth/login`

Public, rate limited to 5 attempts per minute per client IP.

```json
{ "email": "ann@mail.com", "password": "secret123" }
```

**200 OK**:

```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2026-10-25T15:30:00+00:00",
  "user": { "id": "0193a5b2-7c1e-7d3a-9b2f-3c4d5e6f7a8b", "name": "Ann Lee", "email": "ann@mail.com" }
}
```

Errors: **400** email or password missing, **401** `detail: "Invalid email or password"` (the same for an unknown email and a wrong password), **429** too many attempts.

### `GET /api/auth/me`

Requires a token. Returns the user the token belongs to.

**200 OK**: the same body as register. Errors: **401**.

### `GET /api/auth/status`

Public.

**200 OK**:

```json
{ "status": "ok", "version": "1.0.0" }
```

## Tasks API

Every endpoint requires a token, and works only on the caller's own tasks. Another user's task answers **404**, never 403, so its existence is not revealed.

Task body returned by every endpoint below (`TaskResponse`):

```json
{
  "id": "0193a5b3-1a2b-7c3d-8e4f-5a6b7c8d9e0f",
  "title": "Prepare the demo",
  "description": "Slides and a seeded database",
  "status": "InProgress",
  "dueDate": "2026-10-30",
  "isOverdue": false,
  "createdAt": "2026-10-25T14:30:00+00:00",
  "updatedAt": "2026-10-25T16:05:00+00:00"
}
```

`isOverdue` is true when the status is not `Done` and `dueDate` is before today (UTC). It is computed on every request, never stored.

### `GET /api/tasks?status={status}&page={page}&pageSize={pageSize}`

| Parameter | Default | Rules |
| --- | --- | --- |
| `status` | all | `Todo`, `InProgress` or `Done` |
| `page` | 1 | at least 1 |
| `pageSize` | 10 | 1 to 50 |

Ordered by `dueDate`, then `createdAt`. **200 OK**:

```json
{
  "items": [ { "id": "0193a5b3-1a2b-7c3d-8e4f-5a6b7c8d9e0f", "title": "Prepare the demo", "...": "..." } ],
  "page": 1,
  "pageSize": 10,
  "totalCount": 1
}
```

Errors: **400** invalid query, **401**.

### `GET /api/tasks/{id}`

**200 OK** with a task body. Errors: **401**, **404**.

### `POST /api/tasks`

```json
{ "title": "Prepare the demo", "description": "Slides and a seeded database", "dueDate": "2026-10-30", "status": "Todo" }
```

Rules: title required, 3 to 100 characters after trimming; description optional, at most 1000 characters (blank becomes `null`); due date today or later (UTC); status optional, `Todo` when omitted.

**201 Created** with `Location: /api/tasks/{id}` and the task body. Errors: **400**, **401**.

### `PUT /api/tasks/{id}`

Full replacement: every field is sent, and omitting `description` clears it.

```json
{ "title": "Prepare the demo", "description": null, "dueDate": "2026-10-30", "status": "Done" }
```

Same title and description rules as create. A past due date **is** allowed, so an overdue task can be edited without moving its date. Status changes: `Todo` and `InProgress` can change to each other or to `Done`; a `Done` task can only go back to `Todo`.

**200 OK** with the task body. Errors: **400** (including `Done` → `InProgress`), **401**, **404**.

### `DELETE /api/tasks/{id}`

Hard delete. **204 No Content**. Errors: **401**, **404**.

## Errors

Validation failure, **400**:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "title": [ "Title must be between 3 and 100 characters." ],
    "dueDate": [ "Due date cannot be in the past." ]
  }
}
```

Any other expected failure, for example **404**:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5",
  "title": "Not Found",
  "status": 404,
  "detail": "Task not found."
}
```

Error catalog. The code is the `Error.Code` used in the Application layer and in tests (ADR-0006); the response carries the status, `title` and `detail`.

| Code | HTTP | When |
| --- | --- | --- |
| `validation` | 400 | Input fails validation; field errors in `errors` |
| `auth.email_taken` | 409 | Registering an email that already exists |
| `auth.invalid_credentials` | 401 | Unknown email or wrong password |
| `task.not_found` | 404 | The task does not exist or belongs to another user |
| `task.invalid_status_transition` | 400 | Changing a `Done` task to `InProgress` |

Responses that do not come from a `Result`:

| HTTP | When |
| --- | --- |
| 400 | A domain invariant was broken (a validation gap), mapped by the global exception handler |
| 401 | Missing, invalid or expired token on a protected endpoint |
| 429 | Login rate limit exceeded |
| 500 | Unexpected error; ProblemDetails without a stack trace |

## Other endpoints

| Path | Auth | Purpose |
| --- | --- | --- |
| `/health` | public | 200 when the API can reach the database; used by Docker Compose |
| `/scalar` | public | Interactive API reference for both documents |
| `/openapi/{document}.json` | public | OpenAPI documents `auth-v1` and `tasks-v1` |
