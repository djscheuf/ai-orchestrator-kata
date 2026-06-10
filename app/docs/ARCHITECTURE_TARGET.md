# Target Architecture Exploration

## Project Context

**Purpose:** AI code generation benchmark ("Orchestrator Kata") - evaluates AI orchestrators by tracking PR size per feature as a proxy for token expenditure.

**Domain:** Simple financial/banking checkbook application with closed banking system simulation and basic accounting functionality.

---

## Proposed Tech Stack

| Layer | Technology | Purpose |
|-------|------------|---------|
| Frontend | React + TypeScript | User interface and interactions |
| Backend API | .NET | HTTP API and business logic |
| Database | SQL | Data persistence |
| Analytics Sidecar | Python | Data analytics and reporting |

---

## Core Functional Requirements Summary

### Transaction Display
- List transactions by date and payee
- Show positive/negative amounts
- Display running total based on all transactions
- Support display of up to 50-100 transactions at once
- No pagination required in core app

### Transaction Management
- Add new transactions
- Transactions can only be created for today or past dates
- No delete functionality (immutable ledger concept)
- Each transaction updates the account balance immediately

### Account & Authentication
- Mock password-based authentication
- 3-5 different accounts with mock credentials
- Account isolation: each account sees only their own transactions
- Common transaction log exists across all accounts (backend)

### Business Rules
- Cannot create transactions for more money than is currently available
- Every transaction added must update the account balance
- Transactions cannot be deleted
- Can only set transactions for today or in the past

### Out of Scope (Core App)
- Transaction categorization
- Future/scheduled transactions
- Transaction search and filtering
- Transaction deletion or modification
- Pagination

---

## Coding Standards Overview

### React + TypeScript Standards
- **Component Naming:** PascalCase.tsx (e.g., `TransactionList.tsx`, `AddTransactionForm.tsx`)
- **Component Structure:** Named function exports with explicit prop interfaces
- **Props Interface:** `{ComponentName}Props` naming convention
- **Boolean Props:** `is`, `has`, `should`, `can` prefix (e.g., `isLoading`, `hasError`)
- **Event Handlers:** `on` prefix for props (e.g., `onClick`, `onSubmit`), `handle` prefix for functions (e.g., `handleClick`, `handleSubmit`)
- **No class components, no `any` types, no missing prop interfaces**
- **Forbidden:** Inline object/array literals in JSX, components defined inside other components, array index as key

### TypeScript Standards
- **Strict mode enabled** in all projects
- **Naming:** PascalCase for interfaces/types, camelCase for variables/functions, SCREAMING_SNAKE_CASE for constants
- **No `any` type** (use `unknown` with type guards)
- **No `@ts-ignore`** (use `@ts-expect-error` with comment if required)
- **Imports:** Node → External → Internal → Relative order
- **Zod validation** for runtime type safety

### Python Standards
- **Python 3.10+** unless project requires otherwise
- **Naming:** snake_case for modules/functions, PascalCase for classes, UPPER_SNAKE_CASE for constants
- **Type hints:** Consistent throughout, use `|` union syntax (Python 3.10+)
- **Imports:** Standard library → Third-party → Local application
- **No mutable default arguments, no bare `except:` clauses, no `from module import *`
- **Use Pydantic for validation and serialization**
- **Use async/await for I/O-bound operations**

### General Standards
- **Latest package versions** - always use current stable versions and their configuration patterns
- **Formatter/Linter:** Black or Ruff for Python, Prettier for TypeScript/React
- **Line length:** 88 characters (Black default)
- **Indentation:** 4 spaces (Python), 2 spaces (TypeScript/React)

---

## Monorepo Structure

The `/app` directory will serve as a monorepo containing:
- **Frontend:** React + TypeScript UI
- **Backend:** .NET API
- **Database:** SQL schema and migrations
- **Analytics:** Python sidecar service

---

## Architectural Decisions

### 1. Backend Architecture (.NET)

**Pattern:** CQRS (Command Query Responsibility Segregation) with thin controllers

- Controllers handle HTTP concerns only (routing, status codes, serialization)
- Commands and Queries separate write and read operations
- Service layer implements business logic
- Repository pattern for data access
- See: `ARCHITECTURE_BACKEND.md`

### 2. Database Schema

**Core Tables:**
- **Accounts:** account_name, account_id (UUID), foreign key relationships
- **Transactions:** source_account_id, target_account_id, amount, date
- **Account Security:** username, hashed_password, created_at, last_login_at

**Design Principles:**
- Normalized schema with proper foreign key relationships
- Account ID as UUID for scalability
- Immutable transaction ledger (no deletes)
- See: `ARCHITECTURE_DATABASE.md`

### 3. Frontend Architecture (React + TypeScript)

**State Management:** Stateful hooks and React Context

**Component Organization:**
- Pages: Top-level route components
- Reusable Components: Shared UI components using shadcn/ui
- Dependencies: TanStack Table for transaction display
- See: `ARCHITECTURE_UI.md`

### 4. Authentication & Authorization

**Flow:**
1. User provides username/password at login prompt
2. Backend validates credentials against Account Security table
3. Backend generates bearer token scoped to account ID
4. Token grants access to:
   - View transactions for that account
   - Create new transactions for that account
5. Frontend stores token and includes in API requests
6. Backend validates token on each request

### 5. Data Flow

- **Request:** Frontend → .NET API → Database
- **Response:** Database → .NET API → Frontend
- **Analytics:** Python sidecar reads from shared database

### 6. Account Isolation

- Backend enforces account-level filtering
- Frontend receives only relevant transactions for logged-in account
- Common transaction log maintained server-side
- Bearer token restricts operations to single account

### 7. Error Handling

- Validation at API boundary (.NET)
- Business rule enforcement (balance constraints, date constraints)
- User-friendly error messages to frontend

### 8. Testing Strategy

- Unit tests for business logic (Python, .NET, TypeScript)
- Integration tests for API endpoints
- E2E tests for critical user workflows
- Test data generation utility for mock transactions

---

## Reference Documents

- **`ARCHITECTURE_BACKEND.md`** - .NET API structure, CQRS pattern, controllers, services, repositories
- **`ARCHITECTURE_DATABASE.md`** - SQL schema design, tables, relationships, constraints
- **`ARCHITECTURE_UI.md`** - React component hierarchy, state management, shadcn/ui usage, TanStack Table integration
