# T42 — Generative AI deliverable

- Phase: 4 — Delivery
- Depends on: T33
- Agent: developer + docs-writer
- Design: implementation plan section 16

## Goal
Document the real AI-assisted process used to build this project, as the brief's Generative AI section asks: the prompt, the raw output, how it was validated and corrected, and how edge cases were handled.

## Context
- Implementation plan section 16 (file list, prompt, validation process, checklist of typical flaws)
- ADR-0011; agent reports and `code-reviewer` findings from Phases 1–3; `git log`

## Scope — files you may create or change
- `docs/genai/01-prompt.md`, `02-output-sample.md`, `03-review-log.md`, `04-edge-cases.md`, `05-workflow.md`

## Out of scope
- Code. Examples quote real commits; they are not rewritten.

## Checks
- [ ] `02-output-sample.md` is a real, **unedited** excerpt of a first AI output (controller + service)
- [ ] `03-review-log.md` lists only issues actually observed: issue → how detected → fix → commit hash
- [ ] `04-edge-cases.md` covers authentication, authorization (IDOR / 404), validation, dates and time zones
- [ ] `05-workflow.md` explains spec → design docs → tasks, `CLAUDE.md`, subagents in worktrees, review and human merge
- [ ] Developer reviewed every file (this is a personal account of the process)
- [ ] Conventional commits: `docs(genai): ...`
