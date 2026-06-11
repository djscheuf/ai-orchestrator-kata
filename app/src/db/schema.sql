-- Database Schema for Financial Application (SQL Server)
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
    Id NVARCHAR(255) PRIMARY KEY,
    AccountName NVARCHAR(255) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

-- Index for account lookup by name
CREATE INDEX idx_accounts_name ON Accounts(AccountName);

-- ============================================================
-- Table: Transactions
-- ============================================================
-- Represents a financial transaction between two accounts
-- Transactions are immutable - no UPDATE or DELETE operations allowed

CREATE TABLE Transactions (
    Id NVARCHAR(255) PRIMARY KEY,
    SourceAccountId NVARCHAR(255) NOT NULL,
    TargetAccountId NVARCHAR(255) NOT NULL,
    Amount DECIMAL(19, 2) NOT NULL,
    TransactionDate DATETIME2 NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    -- Foreign key constraints
    CONSTRAINT fk_source_account FOREIGN KEY (SourceAccountId) 
        REFERENCES Accounts(Id) ON DELETE NO ACTION,
    CONSTRAINT fk_target_account FOREIGN KEY (TargetAccountId) 
        REFERENCES Accounts(Id) ON DELETE NO ACTION,
    
    -- Business rule constraints
    CONSTRAINT chk_amount_positive CHECK (Amount > 0),
    CONSTRAINT chk_different_accounts CHECK (SourceAccountId != TargetAccountId)
);

-- Indexes for transaction queries
CREATE INDEX idx_transactions_source ON Transactions(SourceAccountId);
CREATE INDEX idx_transactions_target ON Transactions(TargetAccountId);
CREATE INDEX idx_transactions_date ON Transactions(TransactionDate DESC);

-- Composite indexes for optimized account transaction queries
CREATE INDEX idx_transactions_account_date ON Transactions(
    SourceAccountId, 
    TransactionDate DESC
);
CREATE INDEX idx_transactions_account_date_target ON Transactions(
    TargetAccountId, 
    TransactionDate DESC
);

-- ============================================================
-- Table: AccountSecurity
-- ============================================================
-- Stores authentication credentials and audit information for accounts
-- One-to-one relationship with Accounts table

CREATE TABLE AccountSecurity (
    Id NVARCHAR(255) PRIMARY KEY,
    AccountId NVARCHAR(255) NOT NULL UNIQUE,
    Username NVARCHAR(255) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    LastLoginAt DATETIME2,
    
    -- Foreign key with CASCADE delete (remove security record if account deleted)
    CONSTRAINT fk_account FOREIGN KEY (AccountId) 
        REFERENCES Accounts(Id) ON DELETE CASCADE
);

-- Indexes for authentication queries
CREATE INDEX idx_account_security_username ON AccountSecurity(Username);
CREATE INDEX idx_account_security_account_id ON AccountSecurity(AccountId);
