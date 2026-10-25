# 0001 — Record architecture decisions

- Status: Accepted
- Date: 2026-10-25

## Context
The project is built partly by AI agents working in parallel, each seeing only its own task file. Decisions made in one conversation are invisible to the next one. Reviewers also need to understand why a choice was made, not only what the code does.

## Decision
Record each significant decision as a short ADR in `docs/adr/`, using a simplified MADR format (`0000-template.md`). ADRs are written before the part they govern is implemented. Agents read them before coding and must not contradict an Accepted ADR without a new one that supersedes it.

## Alternatives considered
- Decisions only in the README — mixes setup instructions with rationale, and history is lost when the text is edited.
- Decisions in pull request descriptions — hard to find later, and agents do not read them.
- Full MADR or arc42 — more structure than a project of this size needs.

## Consequences
- Positive: one place to look for "why"; consistent decisions across parallel agents; material for the presentation.
- Negative / accepted trade-offs: a little writing overhead, and ADRs must be kept in sync with the code (checked in T43).
