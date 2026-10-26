# 02 - First AI output and the later fix

This file shows three places where the first version written by the AI was wrong, next to the version that is in the repository now. Each "first version" is reconstructed from the review record and the current code, not copied from a commit, so it shows the logic that was wrong and not the exact text. The "now" snippets are the current files.

All three were found by the `code-reviewer` subagent, a separate model that reviews each branch against the ADRs, the design doc and `CLAUDE.md`. Details are in [03-review-log.md](03-review-log.md).

## 1. Any unique violation reported as "email taken" (T22, `UserRepository.TryAddAsync`)

Design intent (DD-01): return `false` when the unique email index rejects the row, and the service answers 409.

First version: it caught every PostgreSQL unique violation (SQLSTATE 23505).

```csharp
catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
{
    context.Entry(user).State = EntityState.Detached;
    return false;
}
```

Problem: a primary-key collision is also a unique violation. It would have been reported to the client as "email already registered", hiding a real bug.

Now: the filter names the email constraint, so any other violation surfaces as an unexpected error.

```csharp
catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: "ix_users_email" })
```

## 2. A corrupt session read as authenticated (T24, `TokenStorage`)

First version: the stored session was accepted when it parsed as JSON and had a token, and the expiry check was a negated comparison.

```typescript
const expired = Date.parse(session.expiresAt) <= Date.now();
if (expired) { this.clear(); return null; }
return session;
```

Problem: when `expiresAt` is missing or not a date, `Date.parse` returns `NaN`, and `NaN <= Date.now()` is `false`. The session was treated as not expired, so a corrupt value in `sessionStorage` counted as a logged-in user.

Now: the shape is validated and the expiry must be a date in the future. Anything else clears the session.

```typescript
return (
  typeof accessToken === 'string' &&
  typeof user === 'object' &&
  user !== null &&
  typeof expiresAt === 'string' &&
  Date.parse(expiresAt) > Date.now()
);
```

A comparison that must be true to accept (`>`) fails safe with `NaN`; a comparison that must be false to accept (`<=`) does not.

## 3. Wrong error when the token's user no longer exists (T20, `GET /api/auth/me`)

First version: `/me` reused the login error when the user in the token was not found.

```csharp
var user = await users.GetByIdAsync(currentUser.UserId, ct);
return user is null ? _invalidCredentials : ToResponse(user);
```

Problem: the client received `auth.invalid_credentials` with "Invalid email or password" for a request that had no password in it. The message is wrong and the code is misleading for anyone debugging.

Now: a dedicated error, still 401 so the SPA signs the user out.

```csharp
private static readonly Error _invalidToken = new(
    ErrorType.Unauthorized, "auth.invalid_token", "The access token is no longer valid.");

return user is null ? _invalidToken : ToResponse(user);
```

## What these examples show

- The generated code compiled, passed its own tests and looked reasonable. Each flaw was a missing edge case, not a syntax error.
- Two of the three are about failing safe: a wrong "yes" (email taken, authenticated) is worse than a wrong "no".
- Every fix came with a test that fails on the first version.
