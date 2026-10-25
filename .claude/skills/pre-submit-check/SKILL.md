---
name: pre-submit-check
description: Runs the Definition of Done checks for the current branch before review — build, format, tests, lint, scope, commit history and secrets
disable-model-invocation: true
argument-hint: "[task-id]"
---

Check the current branch against its task's Definition of Done. Report only; do not fix anything.

1. Find the task file: `docs/tasks/$ARGUMENTS*.md`, or derive the id from the branch name
   (`feat/<task-id>-<slug>`).
2. Run each check. Skip a check whose folder does not exist yet and say so.
   - Build: `dotnet build backend/TaskManager.slnx` — zero warnings.
   - Format: `dotnet format backend/TaskManager.slnx --verify-no-changes`.
   - Unit tests: `dotnet test backend/TaskManager.slnx --filter "Category!=Integration"`.
   - Integration tests: only if `docker info` succeeds, otherwise report "skipped: Docker not running".
   - Frontend, if `frontend/` changed: `npm run lint`, `npm test`, `npm run build` in `frontend/`.
   - Scope: compare `git diff --name-only main...HEAD` with the task's Scope; list files outside it.
   - Tests: every name under "Tests to write first" exists in the code.
   - History: `git log --oneline main..HEAD` uses Conventional Commits, with `test:` before the matching
     `feat:`.
   - Secrets: search the diff for hard-coded passwords, keys, tokens and connection strings
     (the documented demo credentials are allowed).
3. Report a table: check, result (pass / fail / skipped), details. End with "ready for review: yes / no".
