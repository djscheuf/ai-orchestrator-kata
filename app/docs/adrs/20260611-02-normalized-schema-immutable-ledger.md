# 20260611-02. Normalized Schema with Immutable Transaction Ledger

Date: 2026-06-11

## Status

Accepted

## Context

The system needs to maintain financial transaction records with strict data integrity requirements. Transactions must be auditable and immutable, while accounts need to be managed with proper relationships and constraints. The schema must support efficient queries for account balances and transaction history.

## Decision

Implement a normalized relational schema with three core tables:

- **Accounts:** Stores account metadata (account_id, account_name, timestamps)
- **Transactions:** Immutable ledger of transfers (transaction_id, source/target account IDs, amount, date)
- **AccountSecurity:** Authentication credentials and audit info (username, password_hash, last_login_at)

Key design principles:
- Transactions table is append-only (no UPDATE/DELETE operations)
- Foreign key constraints enforce referential integrity
- CHECK constraints validate business rules at database level (amount > 0, different accounts)
- Composite indexes optimize common queries (account_id + date)
- UUID primary keys support distributed systems

## Consequences

**Positive:**
- Immutable transaction ledger provides audit trail and prevents accidental data loss
- Foreign key constraints prevent orphaned records and ensure data consistency
- Normalized design reduces data redundancy and update anomalies
- CHECK constraints enforce business rules at database level, not just application
- UUID primary keys enable distributed systems and horizontal scaling
- Strategic indexing optimizes common query patterns

**Negative:**
- Normalized schema requires JOINs for balance calculations (performance consideration)
- Immutability requires careful migration strategy for schema changes
- No direct UPDATE capability on transactions complicates corrections (requires reversal transactions)
- More complex than denormalized alternatives for simple queries
