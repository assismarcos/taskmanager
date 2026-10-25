# 04 - Edge cases

How authentication, authorization, validation, data and the frontend handle the awkward cases. Each rule has an acceptance criterion (AC) in [the spec](../spec/user-story.md) and a test.

## Authentication

| Case | Behavior |
| --- | --- |
| Unknown email or wrong password | The same 401 with "Invalid email or password", so the response does not reveal which emails exist (AC-2) |
| Many login attempts | Rate limit of 5 per minute per client IP; the sixth gets 429 |
| Missing, malformed, expired or wrongly signed token | 401 on every task endpoint (AC-9). The token's issuer, audience, signature and lifetime are validated |
| Valid token for a user that no longer exists | `GET /api/auth/me` returns 401 with `auth.invalid_token` |
| New endpoint added later | Protected by default through a fallback authorization policy; public endpoints must opt out |
| Password in logs | Request records override `PrintMembers`, so `ToString()` never prints the password or the access token |
| 401 in the SPA | The error interceptor clears the session and sends the user to the login page |

## Authorization

| Case | Behavior |
| --- | --- |
| Reading, editing or deleting another user's task | 404, never 403, so its existence is not revealed (AC-3) |
| Who owns a task | The user id comes only from `ICurrentUser` (the token), never from the request body or URL |
| Where the filter lives | Inside every repository query, so a service cannot forget it |
| Update or delete of an untracked entity | A guard checks ownership, so the user filter cannot be skipped |
| Proof | End-to-end tests with two users show the 404 |

## Validation

| Case | Behavior |
| --- | --- |
| Where rules live | FluentValidation in Application mirrors the domain invariants. The entity still throws if a rule is broken, and a global handler maps that to 400 |
| Title | Required, 3 to 100 characters after trimming (AC-4) |
| Description | At most 1000 characters; blank becomes `null` |
| Email | Valid, at most 254 characters, stored lowercase, so `Ann@Mail.com` and `ann@mail.com` are one user |
| Password | At least 8 characters with a letter and a digit |
| Status changes | `Todo` and `InProgress` can change to each other or to `Done`; `Done` can only go back to `Todo` (AC-6) |
| Page and page size | Page from 1 to 10000, page size from 1 to 50 |
| `PUT` with missing fields | Rejected; `dueDate` and `status` are required in the request model |
| Field error keys | camelCase, so the form can show each error on the right field |

## Dates and time zones

| Case | Behavior |
| --- | --- |
| Today | "Today" is the UTC date, from the injected `TimeProvider`, never `DateTime.Now` |
| Due date on create | Not before today (AC-5) |
| Due date on update | Not checked, so a task that became overdue can still be edited |
| Overdue | Status is not `Done` and the due date is before today (AC-8). A task due today is not overdue. Computed on each request, never stored |
| Timestamps | Always converted to UTC, because Npgsql rejects other offsets for `timestamptz` |
| Ids | `Guid.CreateVersion7(now)` uses the same injected time |

## Data

| Case | Behavior |
| --- | --- |
| Two registrations with the same email at once | The unique index `ix_users_email` decides; the loser gets 409 (AC-1) |
| Another unique violation | Not reported as "email taken"; it surfaces as an unexpected error |
| Deleting a user | Cascade deletes the user's tasks |
| Deleting a task | Hard delete, 204 |

## Frontend

| Case | Behavior |
| --- | --- |
| Session storage | Token, expiry and user in `sessionStorage` (key `tm.session`), cleared when the tab closes ([ADR-0010](../adr/0010-browser-token-storage.md)) |
| Corrupt or expired session | Treated as logged out and cleared |
| Open redirect | `returnUrl` accepts only same-site paths; `/\evil.com` is rejected |
| Server error without field errors | A generic message is shown, not silence |
| Invalid form | Focus moves to the first invalid field |
| Direct link to a task route while logged out | The guard redirects to login and returns after login |
