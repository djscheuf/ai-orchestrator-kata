# Backend Stream TDD Test Plan

## Overview
Test plan for implementing the backend stream of the "Display and Manage Transactions" feature. This plan follows TDD principles with Red-Green-Refactor cycles.

## Story Context
- **User Story**: Display and manage transactions in core financial application
- **Backend Stream**: Implement JWT authentication, transaction management, and API endpoints
- **Acceptance Criteria**: 5 functional criteria + 3 edge cases

## Test Priorities

### Priority Group 1: JWT Token Generation & Validation (Foundation)
These tests establish the authentication foundation required by all other features.

#### Test 1.1: JWT Token Generation
- **Description**: Generate JWT token with account ID claim
- **Given**: Valid username and password
- **When**: User logs in via POST /api/auth/login
- **Then**: Response contains valid JWT token with account ID claim
- **Acceptance**: Token can be decoded and contains accountId claim
- **Status**: Pending

#### Test 1.2: JWT Token Validation
- **Description**: Validate JWT token on protected endpoints
- **Given**: Valid JWT token in Authorization header
- **When**: Request is made to protected endpoint
- **Then**: Token is validated and request proceeds
- **Acceptance**: Token signature and expiration are verified
- **Status**: Pending

#### Test 1.3: Invalid Token Rejection
- **Description**: Reject requests with invalid or missing tokens
- **Given**: Invalid or missing Authorization header
- **When**: Request is made to protected endpoint
- **Then**: Request returns 401 Unauthorized
- **Acceptance**: Invalid tokens are rejected, missing tokens are rejected
- **Status**: Pending

### Priority Group 2: Authentication Endpoint (POST /api/auth/login)
These tests implement the login endpoint with credential validation.

#### Test 2.1: Successful Login
- **Description**: Login with valid credentials returns token and account info
- **Given**: Valid username and password
- **When**: POST /api/auth/login is called
- **Then**: Response is 200 with token, accountId, and accountName
- **Acceptance**: Response matches LoginResponse contract
- **Status**: Pending

#### Test 2.2: Invalid Credentials Rejection
- **Description**: Login with invalid credentials is rejected
- **Given**: Invalid username or password
- **When**: POST /api/auth/login is called
- **Then**: Response is 401 with error message "Invalid credentials"
- **Acceptance**: Invalid credentials return 401
- **Status**: Pending

#### Test 2.3: Missing Required Fields
- **Description**: Login request with missing fields is rejected
- **Given**: Missing username or password in request body
- **When**: POST /api/auth/login is called
- **Then**: Response is 400 with error message
- **Acceptance**: Missing fields return 400 Bad Request
- **Status**: Pending

### Priority Group 3: Repository Pattern Implementation
These tests establish the data access layer abstraction.

#### Test 3.1: Transaction Repository Interface
- **Description**: ITransactionRepository interface exists with required methods
- **Given**: Repository pattern is implemented
- **When**: Interface is examined
- **Then**: Interface has GetByAccountId, Create, GetAll methods
- **Acceptance**: Repository interface is properly defined
- **Status**: Pending

#### Test 3.2: Account Repository Interface
- **Description**: IAccountRepository interface exists with required methods
- **Given**: Repository pattern is implemented
- **When**: Interface is examined
- **Then**: Interface has GetById, GetAll, GetBalance methods
- **Acceptance**: Repository interface is properly defined
- **Status**: Pending

#### Test 3.3: Account Security Repository Interface
- **Description**: IAccountSecurityRepository interface exists
- **Given**: Repository pattern is implemented
- **When**: Interface is examined
- **Then**: Interface has GetByUsername method
- **Acceptance**: Repository interface is properly defined
- **Status**: Pending

### Priority Group 4: Transaction Business Logic Validation
These tests implement business rule enforcement.

#### Test 4.1: Non-zero Amount Validation
- **Description**: Transaction with zero amount is rejected
- **Given**: Transaction request with amount = 0
- **When**: Transaction validation is performed
- **Then**: Validation fails with error "Amount must be non-zero"
- **Acceptance**: Zero amounts are rejected
- **Status**: Pending

#### Test 4.2: Sufficient Funds Validation
- **Description**: Transaction exceeding available balance is rejected
- **Given**: Account balance is $100, transaction amount is $150
- **When**: Transaction validation is performed
- **Then**: Validation fails with error "Insufficient funds"
- **Acceptance**: Insufficient funds are detected
- **Status**: Pending

#### Test 4.3: Future Date Validation
- **Description**: Transaction with future date is rejected
- **Given**: Transaction date is in the future
- **When**: Transaction validation is performed
- **Then**: Validation fails with error "Future dates not allowed"
- **Acceptance**: Future dates are rejected
- **Status**: Pending

#### Test 4.4: Valid Transaction Passes Validation
- **Description**: Valid transaction passes all validations
- **Given**: Valid amount, sufficient funds, today or past date
- **When**: Transaction validation is performed
- **Then**: Validation passes
- **Acceptance**: Valid transactions are accepted
- **Status**: Pending

### Priority Group 5: Transaction Endpoints (GET & POST)
These tests implement the transaction API endpoints.

#### Test 5.1: GET Transactions for Account
- **Description**: Retrieve all transactions for authenticated account
- **Given**: Authenticated user with account ID
- **When**: GET /api/accounts/{accountId}/transactions is called
- **Then**: Response is 200 with list of transactions and balance
- **Acceptance**: Response matches TransactionListResponse contract
- **Status**: Pending

#### Test 5.2: Transactions Sorted by Date Descending
- **Description**: Returned transactions are sorted by date descending
- **Given**: Multiple transactions exist for account
- **When**: GET /api/accounts/{accountId}/transactions is called
- **Then**: Transactions are sorted by date descending
- **Acceptance**: Most recent transactions appear first
- **Status**: Pending

#### Test 5.3: Account Isolation in GET
- **Description**: User only sees transactions for their account
- **Given**: Multiple accounts exist with transactions
- **When**: User A calls GET /api/accounts/{userAId}/transactions
- **Then**: Response only contains transactions where userA is source or target
- **Acceptance**: Account isolation is enforced
- **Status**: Pending

#### Test 5.4: POST Create Transaction Success
- **Description**: Create new transaction with valid data
- **Given**: Valid transaction request with amount, date, target account
- **When**: POST /api/accounts/{accountId}/transactions is called
- **Then**: Response is 201 with created transaction
- **Acceptance**: Response matches TransactionResponse contract
- **Status**: Pending

#### Test 5.5: POST Create Transaction Validation Errors
- **Description**: Create transaction fails with validation errors
- **Given**: Invalid transaction request (zero amount, future date, etc.)
- **When**: POST /api/accounts/{accountId}/transactions is called
- **Then**: Response is 400 with error message
- **Acceptance**: Validation errors are returned
- **Status**: Pending

#### Test 5.6: POST Create Transaction Insufficient Funds
- **Description**: Create transaction fails with insufficient funds
- **Given**: Account balance is insufficient
- **When**: POST /api/accounts/{accountId}/transactions is called
- **Then**: Response is 422 with error "Insufficient funds"
- **Acceptance**: Insufficient funds error is returned
- **Status**: Pending

### Priority Group 6: Account Balance Calculation
These tests implement real-time balance calculation.

#### Test 6.1: Calculate Balance from Transactions
- **Description**: Balance is calculated as sum of incoming minus outgoing
- **Given**: Account has multiple transactions
- **When**: Balance is calculated
- **Then**: Balance = SUM(incoming) - SUM(outgoing)
- **Acceptance**: Balance calculation is correct
- **Status**: Pending

#### Test 6.2: Empty Account Balance
- **Description**: Account with no transactions has zero balance
- **Given**: New account with no transactions
- **When**: Balance is calculated
- **Then**: Balance is 0
- **Acceptance**: Empty account balance is 0
- **Status**: Pending

#### Test 6.3: Balance Reflects Recent Transactions
- **Description**: Balance updates immediately after transaction creation
- **Given**: Transaction is created
- **When**: Balance is retrieved
- **Then**: Balance reflects the new transaction
- **Acceptance**: Balance is up-to-date
- **Status**: Pending

### Priority Group 7: Unit Tests for Auth & Transaction Logic
These tests provide comprehensive unit test coverage.

#### Test 7.1: Unit Tests for JWT Service
- **Description**: JWT service has comprehensive unit tests
- **Given**: JWT service implementation
- **When**: Unit tests are run
- **Then**: All tests pass (token generation, validation, expiration)
- **Acceptance**: 100% coverage of JWT service
- **Status**: Pending

#### Test 7.2: Unit Tests for Transaction Validator
- **Description**: Transaction validator has comprehensive unit tests
- **Given**: Transaction validator implementation
- **When**: Unit tests are run
- **Then**: All tests pass (all validation rules)
- **Acceptance**: 100% coverage of validation logic
- **Status**: Pending

#### Test 7.3: Unit Tests for Balance Calculator
- **Description**: Balance calculator has comprehensive unit tests
- **Given**: Balance calculator implementation
- **When**: Unit tests are run
- **Then**: All tests pass (various transaction scenarios)
- **Acceptance**: 100% coverage of balance calculation
- **Status**: Pending

### Priority Group 8: Integration Tests
These tests verify end-to-end behavior.

#### Test 8.1: Full Login Flow
- **Description**: Complete login flow from credentials to token
- **Given**: Valid user credentials
- **When**: Full login flow is executed
- **Then**: User receives token and can use protected endpoints
- **Acceptance**: Integration test passes
- **Status**: Pending

#### Test 8.2: Full Transaction Creation Flow
- **Description**: Complete transaction creation flow with validation and persistence
- **Given**: Authenticated user with valid transaction data
- **When**: Full transaction creation flow is executed
- **Then**: Transaction is created, balance is updated, response is returned
- **Acceptance**: Integration test passes
- **Status**: Pending

#### Test 8.3: Full Transaction Retrieval Flow
- **Description**: Complete transaction retrieval flow with filtering and sorting
- **Given**: Authenticated user with multiple transactions
- **When**: Full transaction retrieval flow is executed
- **Then**: Transactions are retrieved, filtered by account, sorted by date
- **Acceptance**: Integration test passes
- **Status**: Pending

## Implementation Order

1. **Phase 1**: JWT Token Generation & Validation (Tests 1.1-1.3)
2. **Phase 2**: Authentication Endpoint (Tests 2.1-2.3)
3. **Phase 3**: Repository Pattern (Tests 3.1-3.3)
4. **Phase 4**: Transaction Validation (Tests 4.1-4.4)
5. **Phase 5**: Transaction Endpoints (Tests 5.1-5.6)
6. **Phase 6**: Balance Calculation (Tests 6.1-6.3)
7. **Phase 7**: Unit Tests (Tests 7.1-7.3)
8. **Phase 8**: Integration Tests (Tests 8.1-8.3)

## Success Criteria

- [ ] All 27 tests are implemented
- [ ] All tests pass
- [ ] Code coverage > 80%
- [ ] No code smells or violations
- [ ] All acceptance criteria from story are met
