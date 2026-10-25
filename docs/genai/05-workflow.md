# 05 - Workflow

How the project was built with Claude Code. The decision is recorded in [ADR-0011](../adr/0011-spec-driven-ai-assisted-workflow.md).

## Spec-Driven Development

1. **Spec**: [docs/spec/user-story.md](../spec/user-story.md), one user story and acceptance criteria AC-1 to AC-9.
2. **Design docs and ADRs**: [docs/design/](../design) says how each feature is built; [docs/adr/](../adr) records decisions.
3. **Task files**: [docs/tasks/](../tasks), one per unit of work, each with a scope, tests to write and checks.
4. **Implementation with Test-Driven Development** per task: failing test (`test:` commit), make it pass (`feat:`), refactor (`refactor:`).

Rules: no code without a task file, no task without a design doc. This matters because agents share no memory. A written contract is the only thing that keeps parallel work compatible.

## CLAUDE.md and path-scoped rules

- `CLAUDE.md` holds the working rules: architecture rules, comment rules, commands, commit style and the definition of done.
- `.claude/rules/` holds conventions loaded for matching paths (.NET code, .NET tests, Angular).
- `.claude/skills/` holds repeated procedures: `tdd-cycle`, `start-task`, `pre-submit-check`, `new-adr`.

## Subagents in isolated worktrees

| Agent | Job |
| --- | --- |
| `backend` | Domain, Application, Infrastructure and Api tasks |
| `frontend` | Angular features, stores and Vitest tests |
| `test-engineer` | Architecture tests and end-to-end API tests |
| `docs-writer` | README, architecture, API and this folder |
| `code-reviewer` | Read-only review of each branch against the ADRs, design docs and `CLAUDE.md` |

Implementation agents work in separate git worktrees, each on its own branch and limited to the files in its task's scope. Shared files (`Program.cs`, `DependencyInjection.cs`, `TaskManager.slnx`, `app.routes.ts`, `app.config.ts`, `.claude/**`) are edited only by the orchestrating session, so parallel branches do not conflict.

## Control

- The main session orchestrates and verifies. The `code-reviewer` is a different model run, so it does not share the author's blind spots.
- **The developer reviews and merges every branch.** Nothing is pushed or merged by an agent.
- `.claude/settings.json` enforces this:
  - deny: `git push`, global installs (`npm -g`, `nvm`, `dotnet tool -g`, global `ng`), reading or editing `.env`, recursive deletes;
  - ask: `git merge`;
  - allow: project-local build, test, format, `dotnet ef`, `npm`, `npx ng`, `docker compose` and read-only or local git commands.

## Environment lessons

| Problem | Cause | Fix |
| --- | --- | --- |
| The newest Angular major would not install | Every Angular 22 release needs a newer Node than the machine has, and global tools could not be changed | Use Angular 21, which runs on the installed Node 22.22.0 (DD-03, ADR-0009) |
| npm 10.9.4 crashed on Vitest's optional peer dependencies | An npm resolver bug | `legacy-peer-deps` in the project `.npmrc`, so the fix stays project-local |
| Restores failed or used the wrong feed | A machine-level package source was inherited by restores | Repository `nuget.config` that clears sources and uses nuget.org |
| Background agents stalled | They cannot answer permission prompts | Commands they need were allowed explicitly in the project settings |
| Worktree agents failed to start | On Windows, worktree isolation needs the session to start from the exact on-disk path casing | Start the session from the correctly cased path |

## Process lesson

Per-task reviews by the `code-reviewer` caught real bugs ([03-review-log.md](03-review-log.md)) but were slow and used many tokens. In the last phase the orchestrator ran its own verification for each task (build, tests, scope check) and one final review covered the whole result. This kept most of the benefit at a lower cost.
