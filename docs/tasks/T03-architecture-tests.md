# T03 — Architecture tests

- Phase: 0 — Foundation
- Depends on: T02
- Agent: test-engineer
- Design: `docs/design/DD-04-runtime-and-quality.md` (section 6)

## Goal
Prove the Clean Architecture dependency rules with tests, so a wrong reference fails the build pipeline instead of a code review.

## Context
- Design: DD-04 section 6 (architecture test list)
- ADRs: ADR-0002, ADR-0008

## Scope — files you may create or change
- `backend/tests/TaskManager.ArchitectureTests/**`

## Out of scope
- Any production code. If a rule fails because of existing code, stop and report it.

## Tests to write first (red) — from DD-04, section 6
- `Domain_ShouldNotDependOnAnyOtherProject`
- `Domain_ShouldNotDependOnEntityFrameworkCore`
- `Application_ShouldNotDependOnInfrastructureOrApi`
- `Application_ShouldNotDependOnEntityFrameworkCoreOrAspNetCore`
- `Infrastructure_ShouldNotDependOnApi`
- `Controllers_ShouldNotDependOnInfrastructure`

Red here means: temporarily point the rule at a dependency that does exist, see it fail, then fix the rule. A test that has never failed proves nothing.

## Definition of done
- [ ] All tests above green with NetArchTest.Rules; failure messages name the offending types
- [ ] Zero build warnings; `dotnet format` clean
- [ ] Only files inside Scope changed
- [ ] Conventional commits: `test(arch): ...`
- [ ] Final report: files changed, tests added, decisions taken, open questions
