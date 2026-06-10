# Backend Architecture (.NET)

## Overview

The backend implements a **CQRS (Command Query Responsibility Segregation)** pattern with thin controllers, separating write operations (Commands) from read operations (Queries). This promotes clean separation of concerns and scalability.

---

## Architecture Pattern: CQRS

### Core Principles

- **Commands:** Handle state-changing operations (create transaction, login)
- **Queries:** Handle read-only operations (get transactions, get account)
- **Thin Controllers:** Route HTTP requests to appropriate handlers
- **Service Layer:** Implements business logic
- **Repository Pattern:** Abstracts data access

### Benefits

- Clear separation between reads and writes
- Independent scaling of read and write paths
- Easier testing of business logic
- Reduced coupling between layers

---

## Layered Architecture

```
┌─────────────────────────────────────┐
│         HTTP Controllers            │
│  (Routing, Status Codes, Binding)   │
└────────────────┬────────────────────┘
                 │
┌────────────────▼────────────────────┐
│   Commands & Queries (Handlers)     │
│  (Orchestration, Validation)        │
└────────────────┬────────────────────┘
                 │
┌────────────────▼────────────────────┐
│      Services (Business Logic)      │
│  (Rules, Calculations, Decisions)   │
└────────────────┬────────────────────┘
                 │
┌────────────────▼────────────────────┐
│    Repositories (Data Access)       │
│  (Database Queries, Persistence)    │
└────────────────┬────────────────────┘
                 │
┌────────────────▼────────────────────┐
│          Database (SQL)             │
└─────────────────────────────────────┘
```

---

## Layer Responsibilities

### 1. Controllers (HTTP Layer)

**Responsibility:** Handle HTTP concerns only

```csharp
[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly IMediator _mediator;
    
    public TransactionsController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TransactionDto>>> GetTransactions()
    {
        var query = new GetTransactionsQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }
    
    [HttpPost]
    public async Task<ActionResult<TransactionDto>> CreateTransaction(
        CreateTransactionRequest request)
    {
        var command = new CreateTransactionCommand(
            request.SourceAccountId,
            request.TargetAccountId,
            request.Amount,
            request.Date);
        
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetTransactions), result);
    }
}
```

**Concerns:**
- Route mapping
- HTTP status codes
- Request/response serialization
- Authentication/authorization attributes

**Anti-patterns:**
- ❌ Business logic in controllers
- ❌ Direct database queries
- ❌ Complex validation logic

### 2. Commands & Queries (CQRS Handlers)

**Responsibility:** Orchestrate operations and coordinate between layers

#### Commands (Write Operations)

```csharp
public record CreateTransactionCommand(
    Guid SourceAccountId,
    Guid TargetAccountId,
    decimal Amount,
    DateTime Date) : IRequest<TransactionDto>;

public class CreateTransactionCommandHandler 
    : IRequestHandler<CreateTransactionCommand, TransactionDto>
{
    private readonly ITransactionService _transactionService;
    private readonly IMapper _mapper;
    
    public CreateTransactionCommandHandler(
        ITransactionService transactionService,
        IMapper mapper)
    {
        _transactionService = transactionService;
        _mapper = mapper;
    }
    
    public async Task<TransactionDto> Handle(
        CreateTransactionCommand request,
        CancellationToken cancellationToken)
    {
        var transaction = await _transactionService.CreateTransactionAsync(
            request.SourceAccountId,
            request.TargetAccountId,
            request.Amount,
            request.Date,
            cancellationToken);
        
        return _mapper.Map<TransactionDto>(transaction);
    }
}
```

#### Queries (Read Operations)

```csharp
public record GetTransactionsQuery(Guid AccountId) : IRequest<IEnumerable<TransactionDto>>;

public class GetTransactionsQueryHandler 
    : IRequestHandler<GetTransactionsQuery, IEnumerable<TransactionDto>>
{
    private readonly ITransactionRepository _repository;
    private readonly IMapper _mapper;
    
    public GetTransactionsQueryHandler(
        ITransactionRepository repository,
        IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }
    
    public async Task<IEnumerable<TransactionDto>> Handle(
        GetTransactionsQuery request,
        CancellationToken cancellationToken)
    {
        var transactions = await _repository.GetByAccountIdAsync(
            request.AccountId,
            cancellationToken);
        
        return _mapper.Map<IEnumerable<TransactionDto>>(transactions);
    }
}
```

**Responsibilities:**
- Validate command/query parameters
- Call appropriate service methods
- Map domain models to DTOs
- Handle cross-cutting concerns (logging, caching)

**Pattern:** Use MediatR for command/query dispatching

### 3. Services (Business Logic Layer).

**Responsibility:** Implement business rules and domain logic

```csharp
public interface ITransactionService
{
    Task<Transaction> CreateTransactionAsync(
        Guid sourceAccountId,
        Guid targetAccountId,
        decimal amount,
        DateTime date,
        CancellationToken cancellationToken = default);
    
    Task<IEnumerable<Transaction>> GetAccountTransactionsAsync(
        Guid accountId,
        CancellationToken cancellationToken = default);
}

public class TransactionService : ITransactionService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IAccountRepository _accountRepository;
    
    public TransactionService(
        ITransactionRepository transactionRepository,
        IAccountRepository accountRepository)
    {
        _transactionRepository = transactionRepository;
        _accountRepository = accountRepository;
    }
    
    public async Task<Transaction> CreateTransactionAsync(
        Guid sourceAccountId,
        Guid targetAccountId,
        decimal amount,
        DateTime date,
        CancellationToken cancellationToken = default)
    {
        // Validate date constraint
        if (date > DateTime.UtcNow.Date)
        {
            throw new InvalidOperationException("Cannot create future-dated transactions");
        }
        
        // Validate amount
        if (amount <= 0)
        {
            throw new InvalidOperationException("Amount must be positive");
        }
        
        // Validate source account has sufficient balance
        var sourceAccount = await _accountRepository.GetByIdAsync(
            sourceAccountId,
            cancellationToken);
        
        if (sourceAccount == null)
        {
            throw new AccountNotFoundException(sourceAccountId);
        }
        
        var currentBalance = await _transactionRepository
            .GetAccountBalanceAsync(sourceAccountId, cancellationToken);
        
        if (currentBalance < amount)
        {
            throw new InsufficientFundsException(sourceAccountId, amount, currentBalance);
        }
        
        // Create transaction
        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            SourceAccountId = sourceAccountId,
            TargetAccountId = targetAccountId,
            Amount = amount,
            Date = date,
            CreatedAt = DateTime.UtcNow
        };
        
        await _transactionRepository.AddAsync(transaction, cancellationToken);
        return transaction;
    }
    
    public async Task<IEnumerable<Transaction>> GetAccountTransactionsAsync(
        Guid accountId,
        CancellationToken cancellationToken = default)
    {
        return await _transactionRepository.GetByAccountIdAsync(
            accountId,
            cancellationToken);
    }
}
```

**Responsibilities:**
- Enforce business rules (balance constraints, date constraints)
- Coordinate between repositories
- Throw domain-specific exceptions
- Calculate derived values (balances, totals)

**Anti-patterns:**
- ❌ Direct database queries
- ❌ HTTP concerns
- ❌ Tight coupling to infrastructure

### 4. Repositories (Data Access Layer)

**Responsibility:** Abstract database operations

```csharp
public interface ITransactionRepository
{
    Task<Transaction> AddAsync(
        Transaction transaction,
        CancellationToken cancellationToken = default);
    
    Task<IEnumerable<Transaction>> GetByAccountIdAsync(
        Guid accountId,
        CancellationToken cancellationToken = default);
    
    Task<decimal> GetAccountBalanceAsync(
        Guid accountId,
        CancellationToken cancellationToken = default);
}

public class TransactionRepository : ITransactionRepository
{
    private readonly ApplicationDbContext _context;
    
    public TransactionRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<Transaction> AddAsync(
        Transaction transaction,
        CancellationToken cancellationToken = default)
    {
        await _context.Transactions.AddAsync(transaction, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return transaction;
    }
    
    public async Task<IEnumerable<Transaction>> GetByAccountIdAsync(
        Guid accountId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Transactions
            .Where(t => t.SourceAccountId == accountId || t.TargetAccountId == accountId)
            .OrderByDescending(t => t.Date)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<decimal> GetAccountBalanceAsync(
        Guid accountId,
        CancellationToken cancellationToken = default)
    {
        var outgoing = await _context.Transactions
            .Where(t => t.SourceAccountId == accountId)
            .SumAsync(t => t.Amount, cancellationToken);
        
        var incoming = await _context.Transactions
            .Where(t => t.TargetAccountId == accountId)
            .SumAsync(t => t.Amount, cancellationToken);
        
        return incoming - outgoing;
    }
}
```

**Responsibilities:**
- Execute database queries
- Map database entities to domain models
- Handle transaction management
- Implement query optimization

**Pattern:** Interface-based for testability and dependency injection

---

## Dependency Injection & Configuration

```csharp
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services)
    {
        // Register repositories
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IAccountSecurityRepository, AccountSecurityRepository>();
        
        // Register services
        services.AddScoped<ITransactionService, TransactionService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IAccountService, AccountService>();
        
        // Register MediatR
        services.AddMediatR(typeof(Program));
        
        // Register AutoMapper
        services.AddAutoMapper(typeof(Program));
        
        return services;
    }
}
```

---

## Key Patterns

### Exception Handling

Use domain-specific exceptions for business rule violations:

```csharp
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}

public class InsufficientFundsException : DomainException
{
    public InsufficientFundsException(Guid accountId, decimal requested, decimal available)
        : base($"Account {accountId} has insufficient funds. Requested: {requested}, Available: {available}")
    {
    }
}

public class AccountNotFoundException : DomainException
{
    public AccountNotFoundException(Guid accountId)
        : base($"Account {accountId} not found")
    {
    }
}
```

### DTOs (Data Transfer Objects)

Separate DTOs from domain models:

```csharp
public record TransactionDto(
    Guid Id,
    Guid SourceAccountId,
    Guid TargetAccountId,
    decimal Amount,
    DateTime Date);

public record CreateTransactionRequest(
    Guid SourceAccountId,
    Guid TargetAccountId,
    decimal Amount,
    DateTime Date);
```

### AutoMapper Configuration

```csharp
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Transaction, TransactionDto>();
        CreateMap<Account, AccountDto>();
    }
}
```

---

## Testing Strategy

### Unit Tests (Services)

Test business logic in isolation:

```csharp
[TestFixture]
public class TransactionServiceTests
{
    private Mock<ITransactionRepository> _transactionRepositoryMock;
    private Mock<IAccountRepository> _accountRepositoryMock;
    private TransactionService _service;
    
    [SetUp]
    public void Setup()
    {
        _transactionRepositoryMock = new Mock<ITransactionRepository>();
        _accountRepositoryMock = new Mock<IAccountRepository>();
        _service = new TransactionService(
            _transactionRepositoryMock.Object,
            _accountRepositoryMock.Object);
    }
    
    [Test]
    public async Task CreateTransaction_WithInsufficientFunds_ThrowsException()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        _accountRepositoryMock
            .Setup(r => r.GetByIdAsync(accountId, default))
            .ReturnsAsync(new Account { Id = accountId });
        
        _transactionRepositoryMock
            .Setup(r => r.GetAccountBalanceAsync(accountId, default))
            .ReturnsAsync(50m);
        
        // Act & Assert
        Assert.ThrowsAsync<InsufficientFundsException>(
            () => _service.CreateTransactionAsync(
                accountId,
                Guid.NewGuid(),
                100m,
                DateTime.UtcNow.Date));
    }
}
```

### Integration Tests (API Endpoints)

Test full request/response cycle:

```csharp
[TestFixture]
public class TransactionsControllerTests
{
    private WebApplicationFactory<Program> _factory;
    private HttpClient _client;
    
    [SetUp]
    public void Setup()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();
    }
    
    [Test]
    public async Task CreateTransaction_WithValidData_ReturnsCreated()
    {
        // Arrange
        var request = new CreateTransactionRequest(
            Guid.NewGuid(),
            Guid.NewGuid(),
            100m,
            DateTime.UtcNow.Date);
        
        // Act
        var response = await _client.PostAsJsonAsync("/api/transactions", request);
        
        // Assert
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
    }
}
```

---

## Summary

The CQRS pattern with thin controllers provides:
- **Clarity:** Each layer has a single responsibility
- **Testability:** Business logic isolated from infrastructure
- **Scalability:** Read and write paths can scale independently
- **Maintainability:** Clear separation of concerns
