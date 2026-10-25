# T00 — Solution, projects, build settings and CI

- Phase: 0 — Foundation
- Depends on: T01
- Agent: orchestrator (main session)
- Design: `docs/design/DD-04-runtime-and-quality.md` (sections 2, 6, 7)

## Goal
Create the empty .NET solution with its ten projects, the build settings that keep the code clean, and a CI pipeline that builds and tests every push. No business code.

## Context
- Design: DD-04 sections 2 (layout, build settings, CI), 6 (test projects) and 7 (test platform question)
- ADRs: ADR-0002 (layout under `backend/`), ADR-0007 (packages we do not use), ADR-0008 (test tools)

## Scope — files you may create or change
- `backend/TaskManager.slnx`
- `backend/Directory.Build.props`, `backend/Directory.Packages.props`
- `backend/src/TaskManager.{Domain,Application,Infrastructure,Api}/*.csproj` (+ the template `Program.cs` for Api, emptied to a minimal host)
- `backend/tests/TaskManager.{Domain.UnitTests,Application.UnitTests,Api.UnitTests,Infrastructure.IntegrationTests,Api.IntegrationTests,ArchitectureTests}/*.csproj`
- `global.json` (pin the .NET 10 SDK, `rollForward: latestFeature`)
- `nuget.config` (clears inherited package sources and uses only nuget.org, so restores never depend on machine-level feeds)
- `.config/dotnet-tools.json` (local tools: `dotnet-ef`, `dotnet-reportgenerator-globaltool`)
- `.editorconfig`, `.gitignore` (add the .NET and Node entries to the existing minimal file)
- `.gitattributes` (`* text=auto eol=lf`, so scripts used in Linux containers never get CRLF)
- `.github/workflows/ci.yml` (`backend` job only; T12 adds `frontend`)
- `CLAUDE.md` (Commands section only, if the test filter syntax changes)
- `docs/design/DD-04-runtime-and-quality.md` section 7 (record the test platform decision)

## Out of scope
- Any entity, interface, DTO or test (T02 onward)
- Frontend (T12), Dockerfiles and compose (T40)

## Steps and checks
- Project references point inward: Application → Domain; Infrastructure → Application; Api → Application, Infrastructure. Each test project references only the project it tests; ArchitectureTests references all four.
- `Directory.Packages.props` lists **every** package the plan needs, so later tasks never edit it: EF Core + Npgsql + `EFCore.NamingConventions`, `Microsoft.Extensions.Identity.Core`, JwtBearer, `Microsoft.AspNetCore.OpenApi`, `Scalar.AspNetCore`, FluentValidation, xUnit v3, Shouldly, NSubstitute, `Microsoft.Extensions.TimeProvider.Testing`, `Testcontainers.PostgreSql`, Respawn, `Microsoft.AspNetCore.Mvc.Testing`, `NetArchTest.Rules`, `coverlet.collector`. Confirm the current versions and licenses.
- Choose the test platform (VSTest or Microsoft.Testing.Platform) and confirm the command that runs only unit tests (DD-04 section 7). Update `CLAUDE.md` if it differs.
- `dotnet build backend/TaskManager.slnx` → zero warnings.
- `dotnet test backend/TaskManager.slnx` → green (no tests yet).
- `dotnet format backend/TaskManager.slnx --verify-no-changes` → clean.
- CI passes on a pull request.

## Definition of done
- [ ] All checks above pass locally and in CI
- [ ] Conventional commits (`build:`, `ci:`, `chore:`)
- [ ] Short summary: package versions chosen, test platform decision
