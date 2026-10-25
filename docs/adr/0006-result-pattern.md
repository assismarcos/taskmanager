# 0006 — Use a Result type for expected failures

- Status: Accepted
- Date: 2026-10-25

## Context
Use cases fail in expected ways: invalid input, task not found, email already taken, invalid credentials. Controllers must translate each one into the right HTTP status and ProblemDetails, consistently, in one place. Exceptions for control flow make that flow hard to read and test.

## Decision
Application services return `Result` / `Result<T>` with a typed `Error(ErrorType Type, string Code, string Message, Details?)`, where `ErrorType` is `Validation`, `NotFound`, `Conflict` or `Unauthorized`. A single Api helper maps `ErrorType` to 400/404/409/401 ProblemDetails (RFC 9457), with field errors in `errors` for validation. Exceptions are reserved for the unexpected. A `DomainException` thrown by an entity invariant is the last line of defense and becomes 400 in the global exception handler; any other exception becomes 500 without a stack trace.

## Alternatives considered
- Exceptions plus middleware mapping — less code in services, but the possible failures are not visible in method signatures.
- A library (FluentResults, ErrorOr, OneOf) — works, but the needed surface is about 40 lines and we avoid a dependency.
- Returning `IActionResult` from services — couples Application to ASP.NET Core, which the architecture forbids.

## Consequences
- Positive: failure paths are explicit and unit-testable; HTTP mapping is written once; the same error codes appear in tests and docs.
- Negative / accepted trade-offs: every caller has to check `IsSuccess`; a small amount of hand-written infrastructure to maintain.
