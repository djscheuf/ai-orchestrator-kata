-- Database Schema for Financial Application
-- 
-- This schema implements a normalized relational design for a simple banking application
-- with three core tables: Accounts, Transactions, and AccountSecurity.
--
-- Design principles:
-- - Immutable transaction ledger (no updates/deletes on transactions)
-- - Account isolation through foreign keys
-- - Data integrity enforced at database level
-- - Strategic indexes for query optimization

-- ============================================================
-- Table: Accounts
-- ============================================================
-- Represents a financial account in the system

CREATE TABLE Accounts (
    account_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    account_name VARCHAR(255) NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Index for account lookup by name
CREATE INDEX idx_accounts_name ON Accounts(account_name);

-- ============================================================
-- Table: Transactions
-- ============================================================
-- Represents a financial transaction between two accounts
-- Transactions are immutable - no UPDATE or DELETE operations allowed

CREATE TABLE Transactions (
    transaction_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    source_account_id UUID NOT NULL,
    target_account_id UUID NOT NULL,
    amount DECIMAL(19, 2) NOT NULL,
    transaction_date DATE NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    -- Foreign key constraints
    CONSTRAINT fk_source_account FOREIGN KEY (source_account_id) 
        REFERENCES Accounts(account_id) ON DELETE RESTRICT,
    CONSTRAINT fk_target_account FOREIGN KEY (target_account_id) 
        REFERENCES Accounts(account_id) ON DELETE RESTRICT,
    
    -- Business rule constraints
    CONSTRAINT chk_amount_positive CHECK (amount > 0),
    CONSTRAINT chk_different_accounts CHECK (source_account_id != target_account_id)
);

-- Indexes for transaction queries
CREATE INDEX idx_transactions_source ON Transactions(source_account_id);
CREATE INDEX idx_transactions_target ON Transactions(target_account_id);
CREATE INDEX idx_transactions_date ON Transactions(transaction_date DESC);

-- Composite indexes for optimized account transaction queries
CREATE INDEX idx_transactions_account_date ON Transactions(
    source_account_id, 
    transaction_date DESC
);
CREATE INDEX idx_transactions_account_date_target ON Transactions(
    target_account_id, 
    transaction_date DESC
);

-- ============================================================
-- Table: AccountSecurity
-- ============================================================
-- Stores authentication credentials and audit information for accounts
-- One-to-one relationship with Accounts table

CREATE TABLE AccountSecurity (
    security_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    account_id UUID NOT NULL UNIQUE,
    username VARCHAR(255) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    last_login_at TIMESTAMP,
    
    -- Foreign key with CASCADE delete (remove security record if account deleted)
    CONSTRAINT fk_account FOREIGN KEY (account_id) 
        REFERENCES Accounts(account_id) ON DELETE CASCADE
);

-- Indexes for authentication queries
CREATE INDEX idx_account_security_username ON AccountSecurity(username);
CREATE INDEX idx_account_security_account_id ON AccountSecurity(account_id);
