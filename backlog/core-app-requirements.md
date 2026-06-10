# Core Application Requirements

## Project Overview

**Purpose:** AI code generation benchmark ("Orchestrator Kata")

**Evaluation Approach:** Track PR size per feature as a proxy for token expenditure. Multiple independent features allow evaluation of different AI tools by comparing the output quality and PR size for each feature implementation.

## Domain & Application Type

- Simple financial/banking checkbook application
- Closed banking system simulation
- Basic accounting functionality

## Tech Stack

- **Frontend:** React
- **Backend:** .NET
- **Database:** SQL
- **Analytics:** Python sidecar for data analytics

## Core Functional Requirements

### Transaction Display
- List transactions by date and payee
- Show positive/negative amounts
- Display running total based on all transactions
- Support display of up to 50-100 transactions at once
- No pagination required
- No search functionality in core app (future feature)

### Transaction Management
- Add new transactions
- Transactions can only be created for today or past dates
- No delete functionality (immutable ledger concept)
- Each transaction updates the account balance immediately

### Transaction Data Model
- Date
- Payee/recipient
- Amount (positive or negative)
- Associated account(s)

## Account & Authentication

### Mock Authentication System
- Simple password-based authentication
- 3-5 different accounts with mock credentials
- Example: Account A (password: "123"), Account B (password: "456")

### Account Isolation
- Each account sees only their own transactions
- Transactions shown are "the ones where they are engaged in the transaction"
- Common transaction log exists across all accounts (backend)
- Users only see filtered view of transactions impacting their account

## Data Requirements

### Mock Data Generation Utility
- Generate between 100-1,000 mock transactions per account
- Support 3-5 different accounts
- Utility should be separate/supporting infrastructure

### Data Volume
- System must handle display of up to 5,000-10,000 transactions at once
- Common transaction log across all accounts

## Business Rules

### Account Balance Constraints
- Cannot create transactions for more money than is currently available in the account
- Every transaction added must update the account balance
- Account must track "money currently available"

### Transaction Immutability
- Transactions cannot be deleted
- Future consideration: Flag transactions as incorrect or fraudulent (not in initial scope)

### Transaction Timing
- Can only set transactions for today or in the past
- Cannot create future-dated transactions (this is a separate feature)

## Out of Scope for Core App

The following are explicitly **not** part of the core application and will be implemented as separate features:

- Transaction categorization
- Future/scheduled transactions
- Transaction search and filtering
- Transaction deletion or modification
- Pagination

## Success Criteria

The core application should support:
1. Display existing transactions for logged-in account
2. Add new transactions to existing accounts
3. Show only transactions that impact the current user's account
4. Maintain accurate running balance
5. Enforce business rules (balance constraints, date constraints)
