# Task Manager — User story and acceptance criteria

- Status: Approved
- Date: 2026-10-25
- Source: `docs/implementation-plan-en-us.md`, section 4

This is the **spec** step of Spec-Driven Development. It says what we build and why. How each feature is built lives in `docs/design/`, and the decisions behind it live in `docs/adr/`.

## User story

> **As a** busy professional, **I want to** register, log in and manage my own tasks with a title, description, status and due date, **so that** I can track what I need to do and what is overdue, without seeing anyone else's tasks.

## Acceptance criteria

Each criterion has at least one automated test with the same business-language name. Design docs, task files and tests refer to the criteria by ID.

| ID | Criterion |
| --- | --- |
| AC-1 | I can register with name, email and password; a duplicate email is rejected (409: Conflict). |
| AC-2 | I can log in and receive a token; wrong credentials return 401: Unauthorized with a generic message. |
| AC-3 | I can only see, edit and delete my own tasks; another user's task returns 404: Not Found (its existence is not revealed). |
| AC-4 | Title is required (3 to 100 characters); description has at most 1000 characters. |
| AC-5 | On creation, the due date cannot be in the past. |
| AC-6 | Possible statuses are `Todo`, `InProgress` and `Done`. `Todo` and `InProgress` can change to each other or to `Done`. A `Done` task can only be reopened to `Todo`; changing it from `Done` to `InProgress` is rejected (400: Bad Request). |
| AC-7 | The list can be filtered by status and is paginated, ordered by due date. |
| AC-8 | A task that is not done and whose due date has passed is shown as overdue (`isOverdue`). |
| AC-9 | Without a token, every task endpoint returns 401: Unauthorized. |

## Business rules and clarifications

These rules make the criteria precise enough to test.

| Rule | Criterion |
| --- | --- |
| Name is required, up to 100 characters. | AC-1 |
| Email must be a valid address, up to 254 characters. It is compared and stored in lowercase, so `Ann@Mail.com` and `ann@mail.com` are the same user. | AC-1 |
| Password has at least 8 characters, with at least one letter and one digit. | AC-1 |
| Unknown email and wrong password produce the same message: "Invalid email or password". | AC-2 |
| A token is valid for 60 minutes. There is no refresh: the user logs in again. | AC-2 |
| A task belongs to the user who created it. The owner always comes from the token, never from the request body. | AC-3 |
| Another user's task is treated as if it did not exist. The response is 404: Not Found, never 403: Forbidden. | AC-3 |
| The due date is a calendar date without time. "Past" means before today's date in UTC. | AC-5, AC-8 |
| The past-date check applies only on creation. Editing a task keeps working when its due date has already passed. | AC-5 |
| A task can be created with any status; `Todo` is the default. Setting the status a task already has is allowed. | AC-6 |
| The list starts at page 1, has 10 items per page by default and at most 50. It is ordered by due date, then by creation time. | AC-7 |
| Overdue means status is not `Done` and the due date is before today. A task due today is not overdue. | AC-8 |
| Register, login and status are the only public endpoints. Everything else requires a token. | AC-9 |

## Out of scope

Listed under "Future improvements" in the README: categories, task sharing, attachments, notifications, refresh tokens. Also not included: password reset, email confirmation, profile editing, account deletion and roles.

## Where each criterion is designed and tested

| Criterion | Design docs | Tasks |
| --- | --- | --- |
| AC-1 | DD-01, DD-03 | T10, T11, T20, T22, T24, T31, T33 |
| AC-2 | DD-01, DD-03 | T20, T23, T24, T31, T33 |
| AC-3 | DD-02 | T21, T22, T33 |
| AC-4 | DD-02, DD-03 | T10, T21, T34 |
| AC-5 | DD-02, DD-03 | T10, T21, T33, T34 |
| AC-6 | DD-02, DD-03 | T10, T21, T22, T34 |
| AC-7 | DD-02, DD-03 | T21, T22, T33, T34 |
| AC-8 | DD-02, DD-03 | T10, T21, T33, T34 |
| AC-9 | DD-01, DD-03, DD-04 | T24, T30, T33 |
