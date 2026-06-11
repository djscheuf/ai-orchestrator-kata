using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using FinancialApp.API.Data;
using FinancialApp.API.Repositories;

namespace FinancialApp.API.Tests;

public class ConcurrentTransactionTests : IAsyncLifetime
{
    private ApplicationDbContext _context;
    private SqlTransactionRepository _transactionRepository;
    private SqlAccountRepository _accountRepository;

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        await _context.Database.EnsureCreatedAsync();
        _transactionRepository = new SqlTransactionRepository(_context);
        _accountRepository = new SqlAccountRepository(_context);

        // Seed test accounts
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
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
    }

    [Fact]
    public async Task ConcurrentTransactionCreation_AllTransactionsArePersisted()
    {
        // Arrange
        var transactionCount = 10;
        var tasks = new List<Task<Transaction>>();

        // Act - Create transactions concurrently
        for (int i = 0; i < transactionCount; i++)
        {
            var transaction = new Transaction
            {
                Id = $"txn-{i}",
                SourceAccountId = "account-1",
                TargetAccountId = "account-2",
                Amount = 10m + i,
                TransactionDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            tasks.Add(_transactionRepository.CreateAsync(transaction));
        }

        await Task.WhenAll(tasks);

        // Assert
        var allTransactions = await _transactionRepository.GetAllAsync();
        var transactionList = allTransactions.ToList();

        Assert.Equal(transactionCount, transactionList.Count);
        for (int i = 0; i < transactionCount; i++)
        {
            Assert.Contains(transactionList, t => t.Id == $"txn-{i}");
        }
    }

    [Fact]
    public async Task ConcurrentTransactionCreation_BalanceCalculationIsConsistent()
    {
        // Arrange
        var transactionCount = 5;
        var tasks = new List<Task<Transaction>>();

        // Act - Create transactions concurrently
        for (int i = 0; i < transactionCount; i++)
        {
            var transaction = new Transaction
            {
                Id = $"txn-concurrent-{i}",
                SourceAccountId = "account-1",
                TargetAccountId = "account-2",
                Amount = 100m,
                TransactionDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            tasks.Add(_transactionRepository.CreateAsync(transaction));
        }

        await Task.WhenAll(tasks);

        // Assert - Balance should reflect all transactions
        var balance = await _accountRepository.GetBalanceAsync("account-1");
        Assert.Equal(-500m, balance);
    }

    [Fact]
    public async Task ConcurrentTransactionCreation_DataIntegrityIsPreserved()
    {
        // Arrange
        var account3 = new Account
        {
            Id = "account-3",
            AccountName = "Account 3",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await _context.Accounts.AddAsync(account3);
        await _context.SaveChangesAsync();

        var transactionCount = 8;
        var tasks = new List<Task<Transaction>>();

        // Act - Create transactions from multiple sources to same target concurrently
        for (int i = 0; i < transactionCount; i++)
        {
            var sourceAccount = i % 2 == 0 ? "account-1" : "account-2";
            var transaction = new Transaction
            {
                Id = $"txn-integrity-{i}",
                SourceAccountId = sourceAccount,
                TargetAccountId = "account-3",
                Amount = 50m,
                TransactionDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            tasks.Add(_transactionRepository.CreateAsync(transaction));
        }

        await Task.WhenAll(tasks);

        // Assert - All transactions should be persisted
        var account3Transactions = await _transactionRepository.GetByAccountIdAsync("account-3");
        var transactionList = account3Transactions.ToList();

        Assert.Equal(transactionCount, transactionList.Count);
        Assert.All(transactionList, t => Assert.Equal("account-3", t.TargetAccountId));
    }

    [Fact]
    public async Task ConcurrentTransactionCreation_WithDifferentAccounts_AllTransactionsAreRetrievable()
    {
        // Arrange
        var account4 = new Account
        {
            Id = "account-4",
            AccountName = "Account 4",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await _context.Accounts.AddAsync(account4);
        await _context.SaveChangesAsync();

        var tasks = new List<Task<Transaction>>();

        // Act - Create transactions between different account pairs concurrently
        var transactionPairs = new[] { ("account-1", "account-2"), ("account-2", "account-3"), ("account-3", "account-4") };
        int txnIndex = 0;

        for (int round = 0; round < 3; round++)
        {
            foreach (var (source, target) in transactionPairs)
            {
                var transaction = new Transaction
                {
                    Id = $"txn-pairs-{txnIndex}",
                    SourceAccountId = source,
                    TargetAccountId = target,
                    Amount = 25m,
                    TransactionDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                tasks.Add(_transactionRepository.CreateAsync(transaction));
                txnIndex++;
            }
        }

        await Task.WhenAll(tasks);

        // Assert - Each account should have correct transaction count
        var account1Txns = await _transactionRepository.GetByAccountIdAsync("account-1");
        var account2Txns = await _transactionRepository.GetByAccountIdAsync("account-2");
        var account3Txns = await _transactionRepository.GetByAccountIdAsync("account-3");
        var account4Txns = await _transactionRepository.GetByAccountIdAsync("account-4");

        Assert.Equal(3, account1Txns.Count());
        Assert.Equal(6, account2Txns.Count());
        Assert.Equal(6, account3Txns.Count());
        Assert.Equal(3, account4Txns.Count());
    }
}
