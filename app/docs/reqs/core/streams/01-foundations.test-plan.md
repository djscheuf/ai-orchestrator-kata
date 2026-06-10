# Test Plan: Foundations Stream

## Feature: Database Schema, Seed Data, API Contracts, and Infrastructure

**Source:** `app/docs/reqs/core/streams/01-foundations.stream.json`

**Status:** In Progress

---

## Test Categories

### A. Database Schema Tests (Priority: Critical)

#### A1. Accounts Table Schema
- **Test:** Accounts table exists with correct columns and constraints
- **Verify:** 
  - `account_id` UUID PRIMARY KEY with default gen_random_uuid()
  - `account_name` VARCHAR(255) NOT NULL
  - `created_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
  - `updated_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
- **Status:** ⏳ Pending

#### A2. Transactions Table Schema
- **Test:** Transactions table exists with correct columns and constraints
- **Verify:**
  - `transaction_id` UUID PRIMARY KEY with default gen_random_uuid()
  - `source_account_id` UUID NOT NULL (FK to Accounts)
  - `target_account_id` UUID NOT NULL (FK to Accounts)
  - `amount` DECIMAL(19, 2) NOT NULL
  - `transaction_date` DATE NOT NULL
  - `created_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
  - CHECK constraint: amount > 0
  - CHECK constraint: source_account_id != target_account_id
  - Foreign key constraints with ON DELETE RESTRICT
- **Status:** ⏳ Pending

#### A3. AccountSecurity Table Schema
- **Test:** AccountSecurity table exists with correct columns and constraints
- **Verify:**
  - `security_id` UUID PRIMARY KEY with default gen_random_uuid()
  - `account_id` UUID NOT NULL UNIQUE (FK to Accounts)
  - `username` VARCHAR(255) NOT NULL UNIQUE
  - `password_hash` VARCHAR(255) NOT NULL
  - `created_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
  - `last_login_at` TIMESTAMP (nullable)
  - Foreign key with ON DELETE CASCADE
- **Status:** ⏳ Pending

#### A4. Database Indexes
- **Test:** All required indexes are created
- **Verify:**
  - idx_accounts_name ON Accounts(account_name)
  - idx_transactions_source ON Transactions(source_account_id)
  - idx_transactions_target ON Transactions(target_account_id)
  - idx_transactions_date ON Transactions(transaction_date DESC)
  - idx_transactions_account_date (composite)
  - idx_transactions_account_date_target (composite)
  - idx_account_security_username ON AccountSecurity(username)
  - idx_account_security_account_id ON AccountSecurity(account_id)
- **Status:** ⏳ Pending

#### A5. Schema Constraint Enforcement
- **Test:** Database constraints prevent invalid data
- **Verify:**
  - Cannot insert transaction with amount <= 0
  - Cannot insert transaction with same source and target account
  - Cannot delete account with existing transactions
  - Cannot insert duplicate username in AccountSecurity
  - Cannot insert duplicate account_id in AccountSecurity
- **Status:** ⏳ Pending

---

### B. Seed Data Tests (Priority: Critical)

#### B1. Test Accounts Creation
- **Test:** 5 test accounts are created with proper data
- **Verify:**
  - Accounts: Alice, Bob, Charlie, Diana, Eve
  - Each has valid UUID account_id
  - Each has proper timestamps
  - All accounts are retrievable
- **Status:** ⏳ Pending

#### B2. Account Security Records
- **Test:** Security records exist for all test accounts
- **Verify:**
  - 5 security records created (one per account)
  - Usernames: user_a, user_b, user_c, user_d, user_e
  - Password hashes are bcrypt format
  - One-to-one mapping with accounts
- **Status:** ⏳ Pending

#### B3. Sample Transactions
- **Test:** Sample transactions are created between accounts
- **Verify:**
  - At least 20-50 transactions exist
  - Transactions involve different account pairs
  - Transaction dates are in the past (not future)
  - All amounts are positive
  - No self-transfers exist
- **Status:** ⏳ Pending

#### B4. Account Balance Calculation
- **Test:** Each account has calculable balance from transactions
- **Verify:**
  - Balance = SUM(incoming) - SUM(outgoing)
  - Query returns correct balance for each account
  - No account has negative balance (business rule)
- **Status:** ⏳ Pending

---

### C. API Contract Tests (Priority: High)

#### C1. Authentication Endpoint Contract
- **Test:** POST /api/auth/login contract is defined
- **Verify:**
  - Request schema: { username: string, password: string }
  - Response schema: { token: string, accountId: string, accountName: string }
  - Error responses: 401 Unauthorized, 400 Bad Request
- **Status:** ⏳ Pending

#### C2. Get Transactions Endpoint Contract
- **Test:** GET /api/transactions contract is defined
- **Verify:**
  - Requires Authorization header
  - Response schema: Array of TransactionDto
  - TransactionDto: { id, sourceAccountId, targetAccountId, amount, date, createdAt }
  - Returns only transactions for authenticated account
  - Sorted by date descending
- **Status:** ⏳ Pending

#### C3. Create Transaction Endpoint Contract
- **Test:** POST /api/transactions contract is defined
- **Verify:**
  - Requires Authorization header
  - Request schema: { targetAccountId: string, amount: number, date: string }
  - Response schema: TransactionDto
  - Error responses: 400 Bad Request, 401 Unauthorized, 403 Forbidden
- **Status:** ⏳ Pending

#### C4. Get Account Balance Endpoint Contract
- **Test:** GET /api/accounts/balance contract is defined
- **Verify:**
  - Requires Authorization header
  - Response schema: { accountId: string, accountName: string, balance: number }
- **Status:** ⏳ Pending

---

### D. API Stub Tests (Priority: High)

#### D1. Authentication Stub Returns Mock Data
- **Test:** POST /api/auth/login returns mock token
- **Verify:**
  - Accepts any valid username from seed data
  - Returns mock JWT token
  - Returns correct account ID and name
  - Returns 401 for invalid username
- **Status:** ⏳ Pending

#### D2. Get Transactions Stub Returns Mock Data
- **Test:** GET /api/transactions returns mock transactions
- **Verify:**
  - Returns hardcoded list of transactions
  - Matches contract schema
  - Returns 401 without valid token
- **Status:** ⏳ Pending

#### D3. Create Transaction Stub Returns Mock Data
- **Test:** POST /api/transactions returns mock created transaction
- **Verify:**
  - Accepts valid request body
  - Returns transaction matching input
  - Generates mock transaction ID
  - Returns 400 for invalid input
- **Status:** ⏳ Pending

#### D4. Get Balance Stub Returns Mock Data
- **Test:** GET /api/accounts/balance returns mock balance
- **Verify:**
  - Returns hardcoded balance value
  - Matches contract schema
  - Returns 401 without valid token
- **Status:** ⏳ Pending

---

### E. API Client Tests (Priority: Medium)

#### E1. API Client Authentication Method
- **Test:** Client has login method that calls auth endpoint
- **Verify:**
  - Method accepts username and password
  - Makes POST request to /api/auth/login
  - Returns token and account info
  - Stores token for subsequent requests
- **Status:** ⏳ Pending

#### E2. API Client Get Transactions Method
- **Test:** Client has method to fetch transactions
- **Verify:**
  - Includes Authorization header with token
  - Makes GET request to /api/transactions
  - Returns array of transactions
  - Handles 401 error
- **Status:** ⏳ Pending

#### E3. API Client Create Transaction Method
- **Test:** Client has method to create transaction
- **Verify:**
  - Includes Authorization header with token
  - Makes POST request to /api/transactions
  - Sends correct request body
  - Returns created transaction
  - Handles validation errors
- **Status:** ⏳ Pending

#### E4. API Client Get Balance Method
- **Test:** Client has method to fetch account balance
- **Verify:**
  - Includes Authorization header with token
  - Makes GET request to /api/accounts/balance
  - Returns balance object
  - Handles 401 error
- **Status:** ⏳ Pending

---

## Test Execution Order

### Phase 1: Database Foundation (Tests A1-A5, B1-B4)
1. A1: Accounts table schema ✓
2. A2: Transactions table schema ✓
3. A3: AccountSecurity table schema ✓
4. A4: Database indexes ✓
5. A5: Constraint enforcement ✓
6. B1: Test accounts creation ✓
7. B2: Account security records ✓
8. B3: Sample transactions ✓
9. B4: Balance calculation ✓

### Phase 2: API Contracts (Tests C1-C4)
10. C1: Authentication contract ✓
11. C2: Get transactions contract ✓
12. C3: Create transaction contract ✓
13. C4: Get balance contract ✓

### Phase 3: API Stubs (Tests D1-D4)
14. D1: Authentication stub ✓
15. D2: Get transactions stub ✓
16. D3: Create transaction stub ✓
17. D4: Get balance stub ✓

### Phase 4: API Client (Tests E1-E4)
18. E1: Client authentication ✓
19. E2: Client get transactions ✓
20. E3: Client create transaction ✓
21. E4: Client get balance ✓

---

## Success Criteria

- [ ] All database tables created with correct schema
- [ ] All indexes created
- [ ] All constraints enforced
- [ ] 5 test accounts with security records
- [ ] 20-50 sample transactions
- [ ] All API contracts documented
- [ ] All API stubs return mock data
- [ ] API client implements all methods
- [ ] All tests pass

---

## Current Status

**Phase:** Think (Test Planning)
**Next Action:** Begin Red phase - Write first failing test (A1: Accounts table schema)
