# 0004 — Host the Auth API and the Tasks API in a single process

- Status: Accepted
- Date: 2026-10-25

## Context
The brief asks for a second API that handles user creation and login and has both authorized and non-authorized endpoints. "Second API" could mean a separate service or a separate set of endpoints. The scope is small and the app must start with one command.

## Decision
Treat the second API as a separate set of endpoints in the same ASP.NET Core host. `AuthController` (`/api/auth`) and `TasksController` (`/api/tasks`) are separate controllers, each in its own OpenAPI document (`auth-v1` and `tasks-v1`), shown separately in Scalar. `/api/auth/status` (public) and `/api/auth/me` (JWT) are the non-authorized and authorized pair.

## Alternatives considered
- Two services (an identity service and a tasks service) — shows service boundaries, but adds a second container, shared signing keys or token introspection, CORS between services and more failure modes, with no benefit at this scope.
- One OpenAPI document with tags — simpler, but the two APIs would not be visibly separate as the brief suggests.
- ASP.NET Core Identity endpoints (`MapIdentityApi`) — less code, but a fixed contract and less control over validation and errors.

## Consequences
- Positive: one container, one deployment, one JWT configuration; the separation is visible in code and in the API docs.
- Negative / accepted trade-offs: the two APIs cannot scale or deploy independently. Splitting later is a move of one controller and its use case.
