# 03 - Review log

Only issues that were actually observed. "How detected" is almost always the `code-reviewer` subagent: a separate model that reviews each branch against the ADRs, the design docs and `CLAUDE.md`, and never edits files. A few were found by the orchestrator's own verification runs (build, tests, manual checks). Commit hashes are not listed here; the `git log` of each task branch shows the `test:`, `feat:` and `refactor:` commits.

## Backend

| Issue | How detected | Fix | Task |
| --- | --- | --- | --- |
| A status change to the same status was a silent no-op, and the `Update` invariants had no tests | Code review | Behavior defined by the design doc and covered by tests for each invariant | T10 |
| `Guid.CreateVersion7()` read the system clock, bypassing the injected time | Code review | `CreateVersion7(now)` uses the same `now` as the timestamps | T10 |
| Timestamps were not normalized to UTC. Npgsql rejects non-UTC offsets for `timestamptz` | Code review | Domain converts `now` to UTC. The same gap was found again in `User.Create` | T10, T11 |
| Architecture tests were first shown red by forbidding `System`, which proves little | Code review | Redone with the real project dependencies, controllers selected by name, namespaces derived from the assemblies | T03 |
| Persistence tests depended on test order, and a design-time factory contained a dummy password | Code review | Database reset per test class; the factory carries no credentials | T11 |
| A password could be printed by the record `ToString()` | Code review | `PrintMembers` override on the request records; the same for `AccessToken` and `LoginResponse` | T20 |
| camelCase property keys broke for `DueDate` in validation errors | Code review | Field names mapped explicitly to the JSON naming | T20 |
| `PUT` without `dueDate` or `status` bound to default values | Code review | Required constructor parameters enforced by the serializer in the API JSON options (`JsonOptionsSetup`) | T30 |
| The list-ordering test could not detect a wrong sort key | Code review | Test data built so only the intended key gives the expected order | T22 |
| Update or delete of an untracked entity would skip the user filter | Code review | Guard that the entity belongs to the current user | T22 |
| Token `exp` and `ExpiresAt` differed by under a second | Code review | One value is the source for both | T23 |
| The seed test did not pin the DD-04 task mix | Code review | Test asserts the exact mix of statuses and overdue tasks | T23 |
| `(page - 1) * pageSize` could overflow for a huge page number | Code review | Page capped at 10000 | T21 |

## Frontend

| Issue | How detected | Fix | Task |
| --- | --- | --- | --- |
| A corrupt session without a parseable `expiresAt` counted as valid (`NaN`) | Code review | Validate the shape and require a future expiry (see [02-output-sample.md](02-output-sample.md)) | T24 |
| `returnUrl` accepted `/\evil.com`, an open redirect | Code review | Only same-site paths are accepted | T24 |
| A 400 response without field errors was swallowed | Code review | A generic error message is shown | T24 |
| Focus did not move to the first invalid field | Code review | Shared helper focuses the first invalid control | T24 |

## Also from the first-output examples

| Issue | How detected | Fix | Task |
| --- | --- | --- | --- |
| Any unique violation reported as "email taken" | Code review | Filter on the `ix_users_email` constraint | T22 |
| `/me` returned the login error when the user no longer exists | Code review | New `auth.invalid_token` | T20 |

## Observations

- Nearly everything was an edge case or a missing test, not a build error. The zero-warning build and the pre-written tests caught the rest.
- Authorization and time handling were the most frequent areas, which matches the checklist in the plan.
- The review is only as good as the rules it is given. Putting the rules in `CLAUDE.md`, the ADRs and the design docs made the findings concrete.
