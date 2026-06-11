# 20260611-05. React Context and Hooks for State Management

Date: 2026-06-11

## Status

Accepted

## Context

The frontend needs to manage authentication state (account ID, token) and transaction data (list, loading state, errors) across multiple components. The application is relatively simple with limited state complexity, and introducing a full state management library (Redux, Zustand) would add unnecessary overhead.

## Decision

Use React Context API with stateful hooks for state management:

- **useState:** For local component state (form inputs, UI toggles)
- **useContext:** For shared state across multiple components (auth, transactions)
- **Custom hooks:** Encapsulate context access (useAuth, useTransactions)
- **Context providers:** Placed at page level to provide state to component subtrees

**Key Contexts:**
- **AuthContext:** Manages accountId, accountName, token, login/logout functions
- **TransactionContext:** Manages transactions array, loading state, error state, refresh/add functions

## Consequences

**Positive:**
- No external dependencies required (built into React)
- Simple and straightforward for small-to-medium state complexity
- Co-located state with components that use it improves maintainability
- Easy to test components with context providers
- Minimal boilerplate compared to Redux or other state management libraries
- Natural fit for React's component tree structure

**Negative:**
- Context causes re-renders of all consumers when value changes (performance issue at scale)
- No built-in devtools for debugging state changes
- Requires custom hooks to avoid prop drilling, adding indirection
- No time-travel debugging or action history
- Difficult to share state across multiple independent component trees
- May require refactoring to Redux/Zustand if application complexity grows significantly
