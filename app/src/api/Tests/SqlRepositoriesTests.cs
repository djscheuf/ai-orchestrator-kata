using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using FinancialApp.API.Data;
using FinancialApp.API.Repositories;

namespace FinancialApp.API.Tests;

public class SqlTransactionRepositoryTests : IAsyncLifetime
{
    private ApplicationDbContext _context;
    private SqlTransactionRepository _repository;

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        await _context.Database.EnsureCreatedAsync();
        _repository = new SqlTransactionRepository(_context);
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
    }

    [Fact]
    public async Task GetByAccountIdAsync_WithTransactionsForAccount_ReturnsAllTransactionsWhereAccountIsSourceOrTarget()
    {
        // Arrange
        var accountId = "account-1";
        var otherAccountId = "account-2";
        var transaction1 = new Transaction
        {
            Id = "txn-1",
            SourceAccountId = accountId,
            TargetAccountId = otherAccountId,
            Amount = 100m,
            TransactionDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        var transaction2 = new Transaction
        {
            Id = "txn-2",
            SourceAccountId = otherAccountId,
            TargetAccountId = accountId,
            Amount = 50m,
            TransactionDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        var transaction3 = new Transaction
        {
            Id = "txn-3",
            SourceAccountId = "account-3",
            TargetAccountId = "account-4",
            Amount = 200m,
            TransactionDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Transactions.AddRangeAsync(transaction1, transaction2, transaction3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByAccountIdAsync(accountId);

        // Assert
        var transactions = result.ToList();
        Assert.Equal(2, transactions.Count);
        Assert.Contains(transactions, t => t.Id == "txn-1");
        Assert.Contains(transactions, t => t.Id == "txn-2");
        Assert.DoesNotContain(transactions, t => t.Id == "txn-3");
    }

    [Fact]
    public async Task GetAllAsync_WithTransactions_ReturnsAllTransactions()
    {
        // Arrange
        var transaction1 = new Transaction
        {
            Id = "txn-1",
            SourceAccountId = "account-1",
            TargetAccountId = "account-2",
            Amount = 100m,
            TransactionDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        var transaction2 = new Transaction
        {
            Id = "txn-2",
            SourceAccountId = "account-2",
            TargetAccountId = "account-3",
            Amount = 50m,
            TransactionDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Transactions.AddRangeAsync(transaction1, transaction2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        var transactions = result.ToList();
        Assert.Equal(2, transactions.Count);
    }

    [Fact]
    public async Task CreateAsync_WithNewTransaction_PersistsToDatabase()
    {
        // Arrange
        var transaction = new Transaction
        {
            Id = "txn-new",
            SourceAccountId = "account-1",
            TargetAccountId = "account-2",
            Amount = 100m,
            TransactionDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var result = await _repository.CreateAsync(transaction);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("txn-new", result.Id);
        
        var savedTransaction = await _context.Transactions.FindAsync("txn-new");
        Assert.NotNull(savedTransaction);
        Assert.Equal(100m, savedTransaction.Amount);
    }
}

public class SqlAccountRepositoryTests : IAsyncLifetime
{
    private ApplicationDbContext _context;
    private SqlAccountRepository _repository;

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        await _context.Database.EnsureCreatedAsync();
        _repository = new SqlAccountRepository(_context);
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingAccount_ReturnsAccount()
    {
        // Arrange
        var account = new Account
        {
            Id = "account-1",
            AccountName = "Test Account",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _context.Accounts.AddAsync(account);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync("account-1");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("account-1", result.Id);
        Assert.Equal("Test Account", result.AccountName);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentAccount_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByIdAsync("non-existent");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_WithAccounts_ReturnsAllAccounts()
    {
        // Arrange
        var account1 = new Account
        {
            Id = "account-1",
            AccountName = "Account 1",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var account2 = new Account
        {
            Id = "account-2",
            AccountName = "Account 2",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _context.Accounts.AddRangeAsync(account1, account2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        var accounts = result.ToList();
        Assert.Equal(2, accounts.Count);
    }

    [Fact]
    public async Task GetBalanceAsync_WithTransactions_CalculatesCorrectBalance()
    {
        // Arrange
        var accountId = "account-1";
        var account = new Account
        {
            Id = accountId,
            AccountName = "Test Account",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var transaction1 = new Transaction
        {
            Id = "txn-1",
            SourceAccountId = accountId,
            TargetAccountId = "account-2",
            Amount = 100m,
            TransactionDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        var transaction2 = new Transaction
        {
            Id = "txn-2",
            SourceAccountId = "account-2",
            TargetAccountId = accountId,
            Amount = 50m,
            TransactionDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Accounts.AddAsync(account);
        await _context.Transactions.AddRangeAsync(transaction1, transaction2);
        await _context.SaveChangesAsync();

        // Act
        var balance = await _repository.GetBalanceAsync(accountId);

        // Assert
        Assert.Equal(-50m, balance);
    }

    [Fact]
    public async Task GetBalanceAsync_WithNoTransactions_ReturnsZero()
    {
        // Arrange
        var accountId = "account-1";
        var account = new Account
        {
            Id = accountId,
            AccountName = "Test Account",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _context.Accounts.AddAsync(account);
        await _context.SaveChangesAsync();

        // Act
        var balance = await _repository.GetBalanceAsync(accountId);

        // Assert
        Assert.Equal(0m, balance);
    }
}
