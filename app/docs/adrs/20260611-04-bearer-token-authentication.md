# 20260611-04. Bearer Token Authentication with Account Isolation

Date: 2026-06-11

## Status

Accepted

## Context

The system needs to authenticate users and isolate account data so each user can only access their own transactions. The application uses mock password-based authentication with 3-5 different accounts. Authentication tokens must be scoped to prevent unauthorized access to other accounts' data.

## Decision

Implement bearer token authentication with account-level isolation:

1. **Login Flow:**
   - User provides username/password at login prompt
   - Backend validates credentials against AccountSecurity table
   - Backend generates bearer token scoped to account ID
   - Token grants access to view and create transactions for that account only

2. **Token Usage:**
   - Frontend stores token in localStorage
   - Frontend includes token in Authorization header: `Bearer {token}`
   - Backend validates token on each request

3. **Account Isolation:**
   - Backend enforces account-level filtering on all queries
   - Frontend receives only relevant transactions for logged-in account
   - Common transaction log maintained server-side
   - Bearer token restricts operations to single account

## Consequences

**Positive:**
- Simple to implement and understand compared to OAuth/OpenID Connect
- Token-based approach enables stateless backend scaling
- Account isolation enforced at both API and database query levels
- localStorage provides convenient token persistence for SPAs
- Clear separation between authentication (who you are) and authorization (what you can access)

**Negative:**
- Bearer tokens in localStorage vulnerable to XSS attacks (consider httpOnly cookies for production)
- No built-in token refresh mechanism (requires additional implementation for long-lived tokens)
- Mock authentication insufficient for production use (needs proper password hashing, MFA)
- Token expiration not specified (could lead to stale tokens or security issues)
- No logout mechanism to invalidate tokens server-side (requires token blacklist or short expiration)
