# 0010 — Store the access token in sessionStorage

- Status: Accepted
- Date: 2026-10-25

## Context
The SPA receives a JWT on login (ADR-0005) and must send it on every API call. The token has to survive a page reload but should not outlive the browsing session more than necessary. Every browser storage option trades XSS exposure against CSRF and setup complexity.

## Decision
Keep the token, its expiry and the user in `sessionStorage` (key `tm.session`). It is read by `TokenStorage`, added to `/api` requests by `authInterceptor`, cleared on logout and on any 401, and ignored once expired.

## Alternatives considered
- `httpOnly` secure cookie — not readable by scripts, so safer against XSS; requires CSRF protection, cookie configuration and a different API contract. The recommended production change.
- `localStorage` — survives closing the browser, so a stolen token stays usable longer and is shared across tabs.
- Memory only — safest against persistence, but a page reload logs the user out.

## Consequences
- Positive: simple, survives reloads, cleared when the tab closes, no CSRF surface.
- Negative / accepted trade-offs: readable by any script on the page if an XSS bug exists. Mitigated by Angular's default output escaping, no `innerHTML` with user data, and a 60-minute expiry. Listed as a production improvement in the README.
