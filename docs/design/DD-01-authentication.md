# DD-01 — Authentication

- Status: Approved
- Spec: `docs/spec/user-story.md` (AC-1, AC-2, AC-9)
- Related ADRs: ADR-0004, ADR-0005, ADR-0006, ADR-0007
- Plan reference: implementation plan sections 6, 7 and 8

## 1. Goal and scope

Delivers the **Auth API** (`/api/auth`): registration, login with a JWT access token, the authorized `GET /me` and the public `GET /status`. It also delivers what every other endpoint relies on: token validation, a secure-by-default authorization policy and the current user (`ICurrentUser`). This is the "second API" required by the brief (ADR-0004).

Out of scope: refresh tokens, server-side logout (the client discards the token), password reset, email confirmation, roles.

## 2. Proposed solution

**Register**
1. `AuthController.Register` calls `IAuthService.RegisterAsync(RegisterRequest)`.
2. `RegisterRequestValidator` (FluentValidation) returns a `Validation` error for invalid input.
3. The service hashes the password (`IPasswordHasher`) and builds the user with `User.Create(name, email, hash, now)`. The entity trims and lowercases the email.
4. `IUserRepository.TryAddAsync(user)` saves it. It returns `false` when the unique email index rejects the row, and the service returns `Conflict`. Because the database decides, two simultaneous registrations with the same email cannot both succeed.
5. The controller returns 201 with `UserResponse` and `Location: /api/auth/me`.

**Login**
1. `LoginRequestValidator` checks that email and password are present.
2. `IUserRepository.GetByEmailAsync(normalizedEmail)` loads the user.
3. An unknown email and a wrong password (`IPasswordHasher.Verify` returns false) produce the **same** `auth.invalid_credentials` error, so the response does not reveal which emails exist.
4. `ITokenService.CreateToken(user)` returns the token and its expiry. The controller returns 200 with `LoginResponse`.

**Me**: `ICurrentUser.UserId` (the `sub` claim) is used to load the user and return `UserResponse`. If the user no longer exists, the result is `Unauthorized` with the code `auth.invalid_token`.

**Every other request**: JwtBearer validates issuer, audience, signature and lifetime (clock skew 30 s). A **fallback authorization policy** requires an authenticated user on every endpoint. Only `register`, `login`, `status`, `/health` and the OpenAPI/Scalar endpoints opt out with `[AllowAnonymous]`. A new endpoint is therefore protected unless someone explicitly opens it (AC-9).

## 3. Contracts

| Verb | Route | Auth | Body | Success | Errors |
| --- | --- | --- | --- | --- | --- |
| POST | `/api/auth/register` | public | `RegisterRequest` | 201 + `UserResponse`, `Location: /api/auth/me` | 400, 409 |
| POST | `/api/auth/login` | public, rate limited | `LoginRequest` | 200 + `LoginResponse` | 400, 401, 429 |
| GET | `/api/auth/me` | JWT | — | 200 + `UserResponse` | 401 |
| GET | `/api/auth/status` | public | — | 200 + `{ status: "ok", version }` | — |

All endpoints belong to the `auth-v1` OpenAPI document.

```csharp
// TaskManager.Application/Auth
public sealed record RegisterRequest(string Name, string Email, string Password);
public sealed record LoginRequest(string Email, string Password);
public sealed record UserResponse(Guid Id, string Name, string Email);
public sealed record LoginResponse(string AccessToken, DateTimeOffset ExpiresAt, UserResponse User);

public interface IAuthService
{
    Task<Result<UserResponse>> RegisterAsync(RegisterRequest request, CancellationToken ct);
    Task<Result<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken ct);
    Task<Result<UserResponse>> GetCurrentUserAsync(CancellationToken ct);
}

// TaskManager.Application/Abstractions (ports)
public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string normalizedEmail, CancellationToken ct);
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<bool> TryAddAsync(User user, CancellationToken ct); // false when the email is taken
}
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string passwordHash, string password);
}
public interface ITokenService { AccessToken CreateToken(User user); }
public sealed record AccessToken(string Token, DateTimeOffset ExpiresAt);
public interface ICurrentUser { Guid UserId { get; } }
```

`StatusResponse(string Status, string Version)` lives in the Api project because no use case is involved.

Errors (`Error` and `ErrorType` are defined in DD-02 section 3 and ADR-0006):

| Code | Type | HTTP |
| --- | --- | --- |
| `validation` (with field errors) | Validation | 400, `ValidationProblemDetails` |
| `auth.email_taken` | Conflict | 409 |
| `auth.invalid_credentials` | Unauthorized | 401, detail "Invalid email or password" |
| `auth.invalid_token` | Unauthorized | 401, detail "The access token is no longer valid." (`/me` when the token's user no longer exists) |
| missing or invalid token | — (JwtBearer) | 401 |
| too many login attempts | — (rate limiter) | 429 |

## 4. Data model

`users` table, from implementation plan section 6:

| Column | Type | Rules |
| --- | --- | --- |
| `id` | uuid | PK, `Guid.CreateVersion7()` |
| `name` | varchar(100) | required |
| `email` | varchar(254) | required, unique index `ix_users_email`, always lowercase |
| `password_hash` | varchar(500) | required, never returned by the API |
| `created_at` | timestamptz | required |

Because the email is always stored in lowercase, a plain unique index is enough (no `citext`).

`User` has private setters and a private constructor that sets every property. `User.Create` uses it, and EF Core uses it to materialize rows.

## 5. Business rules and validation

| Rule | Where | AC |
| --- | --- | --- |
| Name required, at most 100 characters | `RegisterRequestValidator`; `User.Create` invariant | AC-1 |
| Email valid format, at most 254 characters | `RegisterRequestValidator`; length is also a `User.Create` invariant | AC-1 |
| Email trimmed and lowercased | `User.Create` (Domain); `AuthService` normalizes before `GetByEmailAsync` | AC-1 |
| Password at least 8 characters, with a letter and a digit | `RegisterRequestValidator` only; the Domain never sees the plain password | AC-1 |
| Email unique | Database unique index, surfaced as `TryAddAsync == false` → `Conflict` | AC-1 |
| Generic login error | `AuthService`: one error for unknown email and wrong password | AC-2 |
| Token: HMAC-SHA256, 60 minutes, claims `sub`, `email`, `name`, issuer and audience | `JwtTokenService` with `JwtOptions` (`ValidateOnStart`, key of at least 32 characters) | AC-2 |
| User id only from the token | `ICurrentUser` implementation in Api reads `sub` | AC-3, AC-9 |
| Every endpoint requires a token unless marked `[AllowAnonymous]` | Fallback authorization policy in `Program.cs` | AC-9 |
| Login rate limit: 5 attempts per minute per client IP, configurable (`RateLimiting__LoginPermitLimit`) | Rate limiter policy `login` on `AuthController.Login` | — |
| Passwords and tokens are never logged | Everywhere | — |

## 6. Testing approach

| Project | Tests (red first) | AC |
| --- | --- | --- |
| Domain.UnitTests (T10) | `Create_WithMixedCaseEmail_StoresLowercaseEmail`; `Create_WhenNameIsEmpty_ThrowsDomainException`; `Create_WhenNameIsLongerThan100Chars_ThrowsDomainException` | AC-1 |
| Application.UnitTests (T20) | `Register_WithValidRequest_ReturnsUserResponse`; `Register_WhenEmailIsTaken_ReturnsConflict`; `Register_HashesPasswordBeforeSaving`; `RegisterValidator_WhenPasswordHasNoDigit_ReturnsValidationError`; `RegisterValidator_WhenPasswordIsShorterThan8Chars_ReturnsValidationError`; `RegisterValidator_WhenEmailIsInvalid_ReturnsValidationError` | AC-1 |
| | `Login_WithValidCredentials_ReturnsAccessToken`; `Login_WhenEmailIsUnknown_ReturnsInvalidCredentials`; `Login_WhenPasswordIsWrong_ReturnsInvalidCredentials`; `Login_WithMixedCaseEmail_FindsUser` | AC-2 |
| | `GetCurrentUser_ReturnsUserFromToken`; `GetCurrentUser_WhenUserNoLongerExists_ReturnsUnauthorized` | AC-9 |
| Infrastructure.IntegrationTests (T11, T22, T23) | T11: `Users_WithDuplicateEmail_ViolateUniqueIndex`. T22: `TryAdd_WhenEmailIsTaken_ReturnsFalse`; `GetByEmail_ReturnsMatchingUser`. T23: `CreateToken_ContainsSubEmailAndNameClaims`; `CreateToken_ExpiresAfterConfiguredMinutes`; `Hash_ThenVerifyWithSamePassword_ReturnsTrue`; `Verify_WithWrongPassword_ReturnsFalse` | AC-1, AC-2 |
| Api.UnitTests (T31) | `Register_WhenSuccessful_Returns201WithLocation`; `Register_WhenEmailIsTaken_Returns409ProblemDetails`; `Login_WhenSuccessful_Returns200WithToken`; `Login_WhenCredentialsAreInvalid_Returns401ProblemDetails`; `Me_ReturnsCurrentUser`; `Status_ReturnsOkWithVersion` | AC-1, AC-2 |
| Api.IntegrationTests (T33) | `RegisterThenLogin_ReturnsToken`; `Register_WithDuplicateEmail_Returns409`; `Login_WithWrongPassword_Returns401WithGenericMessage`; `Me_WithoutToken_Returns401`; `Me_WithToken_ReturnsUser`; `Status_WithoutToken_Returns200` | AC-1, AC-2, AC-9 |

## 7. Risks and open questions

- The token lives in `sessionStorage` and an XSS bug could read it. Accepted in ADR-0010.
- There is no refresh token, so after 60 minutes the user logs in again. The frontend handles 401 by going to the login page (DD-03).
- An unknown email answers slightly faster than a wrong password, because no hash is verified. Not mitigated: registration already reveals whether an email exists (409 in AC-1).
- The rate limiter keeps its counters in memory, which is fine for a single API container. Integration tests raise the limit through configuration.

## 8. Task breakdown

Primary tasks (implementation plan section 13): **T20**, **T23**, **T31**.
Also contributing: T02 (contracts), T10 (`User` entity), T11 (`users` mapping), T22 (`UserRepository`), T24 (frontend auth, see DD-03), T30 (authentication setup, fallback policy, `ICurrentUser`, rate limiter), T33 (end-to-end tests).
