# Backend API Implementation Notes

## Overview
This document provides implementation notes for the backend API of the Financial Application.

## Architecture Pattern: CQRS
The backend follows the Command Query Responsibility Segregation (CQRS) pattern with the following layers:
- **Controllers**: HTTP routing and status codes
- **Services**: Business logic and validation
- **Repositories**: Data access abstraction
- **Middleware**: Cross-cutting concerns (authentication)

## Key Components

### Authentication (JWT)
- **Service**: `Services/JwtTokenService.cs`
- **Middleware**: `Middleware/JwtMiddleware.cs`
- **Controller**: `Controllers/AuthController.cs`

The JWT service generates tokens with the account ID claim. The middleware validates tokens on protected endpoints.

### Transaction Management
- **Validation**: `Services/TransactionValidationService.cs`
- **Balance**: `Services/BalanceCalculationService.cs`
- **Controller**: `Controllers/TransactionsController.cs`

Transactions are validated for business rules (non-zero amount, sufficient funds, valid date). Balance is calculated in real-time from transaction history.

### Data Access
- **Interfaces**: `Repositories/I*Repository.cs`
- **In-Memory**: `Repositories/InMemory*Repository.cs`

The repository pattern abstracts data access. In-memory implementations are provided for testing. Concrete database implementations should be created for production.

## Testing

### Test Files
- `Tests/JwtTokenServiceTests.cs` - JWT token generation and validation
- `Tests/JwtMiddlewareTests.cs` - JWT middleware behavior
- `Tests/AuthControllerTests.cs` - Authentication endpoint
- `Tests/RepositoryInterfaceTests.cs` - Repository interface contracts
- `Tests/TransactionValidationServiceTests.cs` - Business rule validation
- `Tests/BalanceCalculationServiceTests.cs` - Balance calculation
- `Tests/TransactionEndpointIntegrationTests.cs` - End-to-end integration

### Running Tests
```bash
dotnet test
```

### Test Coverage
- 45 total tests
- Unit tests: 38
- Integration tests: 7
- Coverage areas: Happy path, validation errors, edge cases, error handling

## Configuration

### JWT Settings
- **Secret Key**: Currently hardcoded in `JwtTokenService.cs`
  - Should be moved to configuration for production
  - Minimum 32 characters
- **Expiration**: 30 minutes (configurable)
  - Should be moved to configuration for production

### Database
- Currently using in-memory repositories for testing
- For production, implement concrete repository classes with Entity Framework Core
- Create database migrations for schema

## API Endpoints

### Authentication
```
POST /api/auth/login
  Request: { "username": "string", "password": "string" }
  Response: { "token": "string", "accountId": "string", "accountName": "string" }
  Errors: 400 (missing fields), 401 (invalid credentials)
```

### Transactions
```
GET /api/accounts/{accountId}/transactions
  Headers: Authorization: Bearer {token}
  Response: { "transactions": [...], "balance": decimal }
  Errors: 401 (unauthorized), 404 (account not found)

POST /api/accounts/{accountId}/transactions
  Headers: Authorization: Bearer {token}
  Request: { "targetAccountId": "string", "amount": decimal, "date": "string" }
  Response: { "id": "string", "sourceAccountId": "string", ... }
  Errors: 400 (validation), 401 (unauthorized), 422 (insufficient funds)
```

## Dependency Injection

The following services need to be registered in the DI container:

```csharp
services.AddScoped<IJwtTokenService, JwtTokenService>();
services.AddScoped<IAccountSecurityService, AccountSecurityService>();
services.AddScoped<ITransactionRepository, TransactionRepository>(); // Implement with DB
services.AddScoped<IAccountRepository, AccountRepository>(); // Implement with DB
services.AddScoped<ITransactionValidationService, TransactionValidationService>();
services.AddScoped<IBalanceCalculationService, BalanceCalculationService>();
```

## Middleware Setup

Add JWT middleware to the request pipeline:

```csharp
app.UseMiddleware<JwtMiddleware>();
```

## Error Handling

The API returns appropriate HTTP status codes:
- **200**: Successful GET request
- **201**: Successful POST request (resource created)
- **400**: Bad request (validation error)
- **401**: Unauthorized (missing or invalid token)
- **404**: Not found (resource doesn't exist)
- **422**: Unprocessable entity (business rule violation, e.g., insufficient funds)

## Security Considerations

### Current Implementation
- JWT tokens with account ID claim
- Bearer token validation on protected endpoints
- Mock credential validation

### Production Enhancements Needed
- Password hashing (bcrypt or Argon2)
- Rate limiting for login attempts
- Token refresh mechanism
- HTTPS enforcement
- CORS configuration
- Input validation and sanitization
- SQL injection prevention (via EF Core)
- CSRF protection if needed

## Performance Considerations

### Current Implementation
- Real-time balance calculation from transaction history
- In-memory repositories for testing

### Production Optimizations
- Add caching for frequently accessed data
- Optimize database queries with proper indexing
- Consider materialized views for balance calculation
- Add connection pooling
- Implement pagination for large transaction lists

## Future Enhancements

1. **Database Integration**
   - Implement concrete repository classes with EF Core
   - Create database migrations
   - Add connection pooling

2. **Advanced Features**
   - Transaction filtering and search
   - Pagination for transaction lists
   - Transaction categories
   - Recurring transactions
   - Budget tracking

3. **Monitoring & Logging**
   - Structured logging
   - Request/response logging
   - Performance monitoring
   - Error tracking

4. **API Documentation**
   - Swagger/OpenAPI integration
   - API versioning
   - Deprecation policies

## Development Workflow

### Adding a New Feature
1. Write failing tests (RED phase)
2. Implement minimal code to pass tests (GREEN phase)
3. Refactor and clean up code (REFACTOR phase)
4. Update documentation

### Testing a Feature
```bash
# Run all tests
dotnet test

# Run specific test file
dotnet test --filter "ClassName"

# Run with coverage
dotnet test /p:CollectCoverage=true
```

## Troubleshooting

### JWT Token Issues
- Verify secret key is at least 32 characters
- Check token expiration time
- Ensure Bearer scheme is used in Authorization header

### Transaction Validation Issues
- Verify account balance is sufficient
- Check transaction date is not in the future
- Ensure amount is greater than zero
- Verify source and target accounts are different

### Database Connection Issues
- Verify connection string in configuration
- Check database is running and accessible
- Verify database schema matches entity models

## References

- **CQRS Pattern**: `app/docs/ARCHITECTURE_BACKEND.md`
- **Design Document**: `app/docs/reqs/core/display-and-manage-transactions.design.json`
- **Test Plan**: `app/docs/reqs/core/backend-stream-test-plan.md`
- **Implementation Summary**: `app/docs/reqs/core/backend-stream-implementation-summary.md`
