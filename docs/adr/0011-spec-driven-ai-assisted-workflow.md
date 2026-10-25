# 0011 — Build with a spec-driven, AI-assisted workflow

- Status: Accepted
- Date: 2026-10-25

## Context
The project is built with Claude Code, including subagents working in parallel. Agents do not share memory: each one sees `CLAUDE.md` and the files it is pointed to. Without a written contract, parallel agents produce code that does not fit together, and AI-generated code needs systematic validation. The Generative AI section of the brief asks how the AI output was validated.

## Decision
- **Spec-Driven Development**: spec (`docs/spec/`) → design docs and ADRs (`docs/design/`, `docs/adr/`) → one task file per unit of work (`docs/tasks/`) → implementation with Test-Driven Development. No code without a task file; no task without a design doc.
- **Tooling**:
  - `CLAUDE.md` holds the working rules; `.claude/rules/` holds path-scoped conventions;
  - `.claude/settings.json` enforces permissions (no push, no reading `.env`);
  - `.claude/agents/` defines specialized subagents that run in isolated git worktrees;
  - `.claude/skills/` holds the repeated procedures.
- **Control**: the main session orchestrates. Shared files (`Program.cs`, `DependencyInjection.cs`, `TaskManager.slnx`, `app.routes.ts`, `app.config.ts`) are reserved for it. A `code-reviewer` subagent reviews every branch. **Only the developer merges**, and nobody but the developer pushes.

## Alternatives considered
- Prompting without written specs — fast at first; parallel agents diverge and decisions are lost between sessions.
- Agent teams (sessions coordinating through a shared task list) — evaluated; less control and harder to review.
- One agent doing everything sequentially — simpler, but slower and with one long, compacted context.

## Consequences
- Positive: traceability from acceptance criterion to design doc, task, test and commit; agents work in parallel without conflicts; the `docs/genai/` deliverable is a record of a real process.
- Negative / accepted trade-offs: up-front writing in Phase 0; task files must be kept current; parallelism limited to three or four agents so every diff can be reviewed properly.
