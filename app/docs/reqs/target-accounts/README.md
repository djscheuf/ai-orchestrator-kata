# Target Account Dropdown Feature

## Overview
This directory contains the requirements, analysis, and design documentation for updating the transaction form's target account field from a text input to a dropdown selector.

## Feature Summary
Users can now select a target account from a dropdown list showing account names, instead of typing out a full UUID. This improves user experience and reduces form entry errors.

## Documents

### 1. Story Intent
**File:** `update-transaction-form-with-account-dropdown.intent.json`

Extracted user story with:
- User persona: Transaction User
- Capability: Select target account from dropdown
- Benefit: Avoid typing UUID, reduce errors
- Acceptance criteria (4 criteria in Given-When-Then format)

### 2. Story Analysis
**File:** `update-transaction-form-with-account-dropdown.analysis.json`

Comprehensive analysis including:
- Capability breakdown and affected components
- Edge cases (empty list, loading states, fetch failures, permissions)
- Technical and story dependencies
- Open questions and resolved questions

**Analysis Grade:** 93% (14/15 points)
- Strong: Acceptance criteria quality (3/3), narrative clarity (3/3)
- Resolved: API endpoint and account metadata questions

### 3. Design Document
**File:** `update-transaction-form-with-account-dropdown.design.json`

Complete design specification including:
- User flow with domain events
- Instrumentation events for observability
- Layer responsibilities (frontend, backend, data)
- API contract: `GET /api/accounts`
- Architectural decisions with rationale
- Assumptions and ambiguities
- Testing strategy

**Design Grade:** 93% (14/15 points)
- Exemplary: Architectural justification (3/3), workflow clarity (3/3), contracts (3/3), layer responsibilities (3/3)
- Good: Instrumentation (2/3) - could add more granular logging

## Implementation Scope

### Frontend Changes
- Modify `TransactionForm` component to use dropdown
- Add `useEffect` to fetch accounts on mount
- Handle loading and error states
- Update form validation for dropdown selection

### Backend Changes
- Create new endpoint: `GET /api/accounts`
- Return list of accounts with ID and name
- Add authorization check

### No Changes
- Database schema (Account table already has required fields)
- Transaction creation logic
- Form submission format

## API Contract

### GET /api/accounts
```
Request:
  Method: GET
  Headers: Authorization: Bearer {token}

Response (200 OK):
{
  "accounts": [
    { "id": "550e8400-e29b-41d4-a716-446655440001", "name": "Alice Account" },
    { "id": "550e8400-e29b-41d4-a716-446655440002", "name": "Bob Account" }
  ]
}

Error Responses:
  401 Unauthorized
  500 Internal Server Error
```

## Key Decisions

1. **New Endpoint:** Create dedicated `GET /api/accounts` endpoint following REST conventions and CQRS pattern
2. **Minimal Data:** Return only ID and name (no unnecessary fields)
3. **React Dropdown:** Use React component for better UX than native select
4. **Fresh Fetch:** Fetch accounts on component mount to ensure data freshness
5. **Show All Accounts:** Include user's own account in dropdown for flexibility

## Assumptions

- Accounts list will be reasonable size (<100) for now
- No caching initially; can be optimized later if needed
- Users may transfer between their own accounts
- Empty account list is handled with appropriate UX message

## Next Steps

1. Implement backend endpoint `GET /api/accounts`
2. Create/update frontend types for account list response
3. Modify `TransactionForm` component to use dropdown
4. Update tests for new behavior
5. Integration testing with full flow

## References

- Backend Architecture: `app/docs/ARCHITECTURE_BACKEND.md`
- Current TransactionForm: `app/src/ui/components/TransactionForm.tsx`
- Account Repository: `app/src/api/Repositories/IAccountRepository.cs`
- Accounts Controller: `app/src/api/Controllers/AccountsController.cs`
