# 0005 — Use short-lived JWT bearer tokens and PBKDF2 password hashing

- Status: Accepted
- Date: 2026-10-25

## Context
The SPA and the API need stateless authentication that is easy to test from Scalar and from integration tests. Passwords must be stored safely. Refresh tokens are out of scope (spec).

## Decision
- Issue JWT access tokens signed with HMAC-SHA256, valid for 60 minutes, with claims `sub` (user id), `email` and `name`, plus issuer and audience.
- Read the key from the environment (`Jwt__Key`, at least 32 characters). `JwtOptions` is validated at startup.
- Hash passwords with ASP.NET Core Identity's `PasswordHasher<T>` (PBKDF2 with salt), behind the Application port `IPasswordHasher`.
- Require authentication through a fallback authorization policy, and rate-limit the login endpoint.

## Alternatives considered
- Cookie authentication — safer against XSS when `httpOnly`, but needs CSRF protection and same-site configuration (see ADR-0010).
- Refresh tokens — better UX for long sessions; explicitly out of scope.
- An external identity provider (Entra ID, Auth0, Keycloak) — production-grade, but the brief asks us to build user creation and login.
- BCrypt or Argon2 libraries — good choices, but they add a dependency where the built-in PBKDF2 hasher is adequate.

## Consequences
- Positive: stateless API; simple to call from Scalar and tests; no password hashing code of our own.
- Negative / accepted trade-offs: a token cannot be revoked before it expires; users log in again every hour; the symmetric key must be protected.
