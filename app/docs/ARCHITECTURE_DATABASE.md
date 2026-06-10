# Database Architecture (SQL)

## Overview

The database uses a normalized relational schema with three core tables: **Accounts**, **Transactions**, and **AccountSecurity**. The design emphasizes data integrity, account isolation, and immutable transaction ledger.

---

## Core Tables

### 1. Accounts

Represents a financial account in the system.

```sql
CREATE TABLE Accounts (
    account_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    account_name VARCHAR(255) NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);
```

**Columns:**
- `account_id` (UUID, PK): Unique identifier for the account
- `account_name` (VARCHAR): Display name of the account
- `created_at` (TIMESTAMP): Account creation timestamp
- `updated_at` (TIMESTAMP): Last modification timestamp

**Constraints:**
- Primary key ensures uniqueness
- NOT NULL constraints enforce required fields

**Indexes:**
```sql
CREATE INDEX idx_accounts_name ON Accounts(account_name);
```

---

### 2. Transactions

Represents a financial transaction between two accounts.

```sql
CREATE TABLE Transactions (
    transaction_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    source_account_id UUID NOT NULL,
    target_account_id UUID NOT NULL,
    amount DECIMAL(19, 2) NOT NULL,
    transaction_date DATE NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_source_account FOREIGN KEY (source_account_id) 
        REFERENCES Accounts(account_id) ON DELETE RESTRICT,
    CONSTRAINT fk_target_account FOREIGN KEY (target_account_id) 
        REFERENCES Accounts(account_id) ON DELETE RESTRICT,
    CONSTRAINT chk_amount_positive CHECK (amount > 0),
    CONSTRAINT chk_different_accounts CHECK (source_account_id != target_account_id)
);
```

**Columns:**
- `transaction_id` (UUID, PK): Unique identifier for the transaction
- `source_account_id` (UUID, FK): Account sending money
- `target_account_id` (UUID, FK): Account receiving money
- `amount` (DECIMAL): Transaction amount (positive values only)
- `transaction_date` (DATE): Date of the transaction
- `created_at` (TIMESTAMP): Record creation timestamp

**Constraints:**
- Foreign keys link to Accounts table
- ON DELETE RESTRICT prevents account deletion if transactions exist
- CHECK constraint ensures amount is positive
- CHECK constraint prevents self-transfers

**Indexes:**
```sql
CREATE INDEX idx_transactions_source ON Transactions(source_account_id);
CREATE INDEX idx_transactions_target ON Transactions(target_account_id);
CREATE INDEX idx_transactions_date ON Transactions(transaction_date DESC);
CREATE INDEX idx_transactions_account_date ON Transactions(
    source_account_id, 
    transaction_date DESC
);
CREATE INDEX idx_transactions_account_date_target ON Transactions(
    target_account_id, 
    transaction_date DESC
);
```

**Design Notes:**
- Immutable: No UPDATE or DELETE operations allowed
- Amount always positive; direction determined by source/target
- Composite indexes optimize common queries (account + date)

---

### 3. AccountSecurity

Stores authentication credentials and audit information for accounts.

```sql
CREATE TABLE AccountSecurity (
    security_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    account_id UUID NOT NULL UNIQUE,
    username VARCHAR(255) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    last_login_at TIMESTAMP,
    CONSTRAINT fk_account FOREIGN KEY (account_id) 
        REFERENCES Accounts(account_id) ON DELETE CASCADE
);
```

**Columns:**
- `security_id` (UUID, PK): Unique identifier for security record
- `account_id` (UUID, FK, UNIQUE): Links to Accounts table (one-to-one)
- `username` (VARCHAR, UNIQUE): Login username
- `password_hash` (VARCHAR): Bcrypt or similar hash of password
- `created_at` (TIMESTAMP): Security record creation timestamp
- `last_login_at` (TIMESTAMP, NULLABLE): Last successful login timestamp

**Constraints:**
- UNIQUE on account_id ensures one security record per account
- UNIQUE on username ensures usernames are globally unique
- ON DELETE CASCADE removes security record if account is deleted
- Foreign key maintains referential integrity

**Indexes:**
```sql
CREATE INDEX idx_account_security_username ON AccountSecurity(username);
CREATE INDEX idx_account_security_account_id ON AccountSecurity(account_id);
```

**Design Notes:**
- Password stored as hash only (never plaintext)
- last_login_at tracks audit information
- One-to-one relationship with Accounts

---

## Entity Relationship Diagram

```
┌─────────────────────┐
│     Accounts        │
├─────────────────────┤
│ account_id (PK)     │
│ account_name        │
│ created_at          │
│ updated_at          │
└──────────┬──────────┘
           │
           ├─────────────────────────────────┐
           │                                 │
           │ (1:1)                           │ (1:N)
           │                                 │
    ┌──────▼──────────────┐    ┌────────────▼──────────────┐
    │  AccountSecurity    │    │    Transactions           │
    ├─────────────────────┤    ├───────────────────────────┤
    │ security_id (PK)    │    │ transaction_id (PK)       │
    │ account_id (FK)     │    │ source_account_id (FK)    │
    │ username            │    │ target_account_id (FK)    │
    │ password_hash       │    │ amount                    │
    │ created_at          │    │ transaction_date          │
    │ last_login_at       │    │ created_at                │
    └─────────────────────┘    └───────────────────────────┘
```

---

## Key Queries

### Get Account Balance

```sql
SELECT 
    a.account_id,
    a.account_name,
    COALESCE(SUM(CASE WHEN t.target_account_id = a.account_id THEN t.amount ELSE 0 END), 0) -
    COALESCE(SUM(CASE WHEN t.source_account_id = a.account_id THEN t.amount ELSE 0 END), 0) AS balance
FROM Accounts a
LEFT JOIN Transactions t ON (
    t.source_account_id = a.account_id OR 
    t.target_account_id = a.account_id
)
WHERE a.account_id = $1
GROUP BY a.account_id, a.account_name;
```

### Get Account Transactions (Paginated)

```sql
SELECT 
    transaction_id,
    source_account_id,
    target_account_id,
    amount,
    transaction_date,
    created_at
FROM Transactions
WHERE source_account_id = $1 OR target_account_id = $1
ORDER BY transaction_date DESC, created_at DESC
LIMIT $2 OFFSET $3;
```

### Get Running Balance at Date

```sql
SELECT 
    transaction_date,
    SUM(CASE WHEN target_account_id = $1 THEN amount ELSE -amount END) 
        OVER (ORDER BY transaction_date, created_at) AS running_balance
FROM Transactions
WHERE source_account_id = $1 OR target_account_id = $1
ORDER BY transaction_date DESC, created_at DESC;
```

### Authenticate User

```sql
SELECT 
    a.account_id,
    a.account_name,
    s.password_hash
FROM Accounts a
INNER JOIN AccountSecurity s ON a.account_id = s.account_id
WHERE s.username = $1;
```

### Update Last Login

```sql
UPDATE AccountSecurity
SET last_login_at = CURRENT_TIMESTAMP
WHERE account_id = $1;
```

---

## Data Integrity Rules

### Business Rules Enforced at Database Level

1. **Amount Validation**
   - CHECK constraint ensures amount > 0
   - Application validates sufficient balance before insert

2. **Account Isolation**
   - Foreign keys ensure referenced accounts exist
   - Application filters transactions by account_id

3. **Transaction Immutability**
   - No UPDATE or DELETE permissions on Transactions table
   - Only INSERT operations allowed

4. **Self-Transfer Prevention**
   - CHECK constraint prevents source_account_id = target_account_id

5. **Referential Integrity**
   - Foreign keys prevent orphaned transactions
   - ON DELETE RESTRICT prevents account deletion with transactions

---

## Indexes Strategy

### Primary Indexes (Required)
- `Accounts.account_id` (PK)
- `Transactions.transaction_id` (PK)
- `AccountSecurity.security_id` (PK)

### Foreign Key Indexes (Automatic)
- `Transactions.source_account_id`
- `Transactions.target_account_id`
- `AccountSecurity.account_id`

### Query Optimization Indexes
- `Transactions(source_account_id, transaction_date DESC)` - Get account transactions
- `Transactions(target_account_id, transaction_date DESC)` - Get account transactions
- `AccountSecurity(username)` - Login queries
- `Accounts(account_name)` - Account lookup by name

---

## Migration Strategy

### Initial Schema Creation

```sql
-- Create Accounts table
CREATE TABLE Accounts (
    account_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    account_name VARCHAR(255) NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Create Transactions table
CREATE TABLE Transactions (
    transaction_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    source_account_id UUID NOT NULL,
    target_account_id UUID NOT NULL,
    amount DECIMAL(19, 2) NOT NULL,
    transaction_date DATE NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_source_account FOREIGN KEY (source_account_id) 
        REFERENCES Accounts(account_id) ON DELETE RESTRICT,
    CONSTRAINT fk_target_account FOREIGN KEY (target_account_id) 
        REFERENCES Accounts(account_id) ON DELETE RESTRICT,
    CONSTRAINT chk_amount_positive CHECK (amount > 0),
    CONSTRAINT chk_different_accounts CHECK (source_account_id != target_account_id)
);

-- Create AccountSecurity table
CREATE TABLE AccountSecurity (
    security_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    account_id UUID NOT NULL UNIQUE,
    username VARCHAR(255) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    last_login_at TIMESTAMP,
    CONSTRAINT fk_account FOREIGN KEY (account_id) 
        REFERENCES Accounts(account_id) ON DELETE CASCADE
);

-- Create indexes
CREATE INDEX idx_accounts_name ON Accounts(account_name);
CREATE INDEX idx_transactions_source ON Transactions(source_account_id);
CREATE INDEX idx_transactions_target ON Transactions(target_account_id);
CREATE INDEX idx_transactions_date ON Transactions(transaction_date DESC);
CREATE INDEX idx_transactions_account_date ON Transactions(
    source_account_id, transaction_date DESC
);
CREATE INDEX idx_transactions_account_date_target ON Transactions(
    target_account_id, transaction_date DESC
);
CREATE INDEX idx_account_security_username ON AccountSecurity(username);
CREATE INDEX idx_account_security_account_id ON AccountSecurity(account_id);
```

---

## Mock Data Generation

### Sample Data Insertion

```sql
-- Insert test accounts
INSERT INTO Accounts (account_name) VALUES
    ('Alice Account'),
    ('Bob Account'),
    ('Charlie Account'),
    ('Diana Account'),
    ('Eve Account');

-- Insert security records (passwords hashed in application)
INSERT INTO AccountSecurity (account_id, username, password_hash)
SELECT account_id, 'user_' || LOWER(SUBSTRING(account_name, 1, 1)), 
       '$2b$12$...' -- Bcrypt hash
FROM Accounts;

-- Insert sample transactions
INSERT INTO Transactions (source_account_id, target_account_id, amount, transaction_date)
SELECT 
    a1.account_id,
    a2.account_id,
    (RANDOM() * 500 + 10)::DECIMAL(19, 2),
    CURRENT_DATE - (RANDOM() * 30)::INTEGER
FROM Accounts a1, Accounts a2
WHERE a1.account_id != a2.account_id
LIMIT 100;
```

---

## Performance Considerations

### Query Optimization
- Composite indexes on (account_id, date) for transaction queries
- Separate indexes on foreign keys for join operations
- Avoid full table scans with proper indexing

### Scaling Considerations
- UUID primary keys support distributed systems
- Immutable transactions enable append-only optimization
- Denormalization opportunities (materialized views for balances)

### Backup & Recovery
- Regular backups of Transactions table (immutable, critical)
- Point-in-time recovery capability
- Archive old transactions if needed

---

## Summary

The schema design provides:
- **Data Integrity:** Constraints and foreign keys enforce business rules
- **Account Isolation:** Foreign keys and application logic separate accounts
- **Immutability:** Transaction ledger is append-only
- **Performance:** Strategic indexes optimize common queries
- **Auditability:** Timestamps track creation and modifications
