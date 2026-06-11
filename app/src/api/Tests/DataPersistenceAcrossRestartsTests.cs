using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using FinancialApp.API.Data;
using FinancialApp.API.Repositories;

namespace FinancialApp.API.Tests;

public class DataPersistenceAcrossRestartsTests
{
    [Fact]
    public async Task DataPersistence_TransactionsCreatedBeforeRestart_AreAvailableAfterRestart()
    {
        // Arrange - First "application instance"
        var dbName = "PersistenceTest_" + Guid.NewGuid().ToString();
        var options1 = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        var transactionIds = new List<string>();

        using (var context1 = new ApplicationDbContext(options1))
        {
            await context1.Database.EnsureCreatedAsync();

            var account1 = new Account
            {
                Id = "account-persist-1",
                AccountName = "Persistent Account 1",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            var account2 = new Account
            {
                Id = "account-persist-2",
                AccountName = "Persistent Account 2",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await context1.Accounts.AddRangeAsync(account1, account2);
            await context1.SaveChangesAsync();

            // Create transactions
            for (int i = 0; i < 3; i++)
            {
                var txnId = $"txn-persist-{i}";
                var transaction = new Transaction
                {
                    Id = txnId,
                    SourceAccountId = "account-persist-1",
                    TargetAccountId = "account-persist-2",
                    Amount = 100m + i,
                    TransactionDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                await context1.Transactions.AddAsync(transaction);
                transactionIds.Add(txnId);
            }

            await context1.SaveChangesAsync();
        }

        // Act - Second "application instance" (simulating restart)
        var options2 = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        using (var context2 = new ApplicationDbContext(options2))
        {
            // Assert - Data should still be available
            var transactions = await context2.Transactions.ToListAsync();
            var accounts = await context2.Accounts.ToListAsync();

            Assert.Equal(3, transactions.Count);
            Assert.Equal(2, accounts.Count);

            foreach (var txnId in transactionIds)
            {
                Assert.Contains(transactions, t => t.Id == txnId);
            }
        }
    }

    [Fact]
    public async Task DataPersistence_AccountBalanceIsPreservedAcrossRestarts()
    {
        // Arrange - First "application instance"
        var dbName = "BalancePersistenceTest_" + Guid.NewGuid().ToString();
        var options1 = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        decimal expectedBalance = 0m;

        using (var context1 = new ApplicationDbContext(options1))
        {
            await context1.Database.EnsureCreatedAsync();

            var account1 = new Account
            {
                Id = "account-balance-1",
                AccountName = "Balance Test Account 1",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            var account2 = new Account
            {
                Id = "account-balance-2",
                AccountName = "Balance Test Account 2",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await context1.Accounts.AddRangeAsync(account1, account2);
            await context1.SaveChangesAsync();

            // Create transactions that affect balance
            var transaction1 = new Transaction
            {
                Id = "txn-balance-1",
                SourceAccountId = "account-balance-2",
                TargetAccountId = "account-balance-1",
                Amount = 500m,
                TransactionDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            var transaction2 = new Transaction
            {
                Id = "txn-balance-2",
                SourceAccountId = "account-balance-1",
                TargetAccountId = "account-balance-2",
                Amount = 200m,
                TransactionDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            await context1.Transactions.AddRangeAsync(transaction1, transaction2);
            await context1.SaveChangesAsync();

            // Calculate expected balance: received 500, sent 200 = 300
            expectedBalance = 300m;
        }

        // Act - Second "application instance" (simulating restart)
        var options2 = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        using (var context2 = new ApplicationDbContext(options2))
        {
            var repository = new SqlAccountRepository(context2);

            // Assert - Balance should be preserved
            var balance = await repository.GetBalanceAsync("account-balance-1");
            Assert.Equal(expectedBalance, balance);
        }
    }

    [Fact]
    public async Task DataPersistence_MultipleRestarts_PreserveAllData()
    {
        // Arrange
        var dbName = "MultiRestartTest_" + Guid.NewGuid().ToString();
        var transactionCount = 0;

        // First restart cycle
        using (var context = new ApplicationDbContext(
            new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options))
        {
            await context.Database.EnsureCreatedAsync();

            var account = new Account
            {
                Id = "account-multi-1",
                AccountName = "Multi Restart Account",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            var otherAccount = new Account
            {
                Id = "account-multi-2",
                AccountName = "Other Account",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await context.Accounts.AddRangeAsync(account, otherAccount);
            await context.SaveChangesAsync();

            for (int i = 0; i < 2; i++)
            {
                await context.Transactions.AddAsync(new Transaction
                {
                    Id = $"txn-multi-{i}",
                    SourceAccountId = "account-multi-1",
                    TargetAccountId = "account-multi-2",
                    Amount = 50m,
                    TransactionDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                });
            }
            await context.SaveChangesAsync();
            transactionCount = 2;
        }

        // Second restart cycle
        using (var context = new ApplicationDbContext(
            new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options))
        {
            var existingTransactions = await context.Transactions.CountAsync();
            Assert.Equal(transactionCount, existingTransactions);

            for (int i = 2; i < 4; i++)
            {
                await context.Transactions.AddAsync(new Transaction
                {
                    Id = $"txn-multi-{i}",
                    SourceAccountId = "account-multi-1",
                    TargetAccountId = "account-multi-2",
                    Amount = 50m,
                    TransactionDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                });
            }
            await context.SaveChangesAsync();
            transactionCount = 4;
        }

        // Third restart cycle - verify all data persists
        using (var context = new ApplicationDbContext(
            new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options))
        {
            var allTransactions = await context.Transactions.ToListAsync();
            var allAccounts = await context.Accounts.ToListAsync();

            Assert.Equal(transactionCount, allTransactions.Count);
            Assert.Equal(2, allAccounts.Count);

            for (int i = 0; i < transactionCount; i++)
            {
                Assert.Contains(allTransactions, t => t.Id == $"txn-multi-{i}");
            }
        }
    }

    [Fact]
    public async Task DataPersistence_SeedDataIsNotDuplicatedOnRestart()
    {
        // Arrange
        var dbName = "SeedIdempotencyTest_" + Guid.NewGuid().ToString();
        var seedAccounts = new[]
        {
            new Account 
            { 
                Id = "seed-account-1", 
                AccountName = "Seed Account 1",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Account 
            { 
                Id = "seed-account-2", 
                AccountName = "Seed Account 2",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        // First restart - seed data
        using (var context = new ApplicationDbContext(
            new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options))
        {
            await context.Database.EnsureCreatedAsync();

            foreach (var account in seedAccounts)
            {
                if (!await context.Accounts.AnyAsync(a => a.Id == account.Id))
                {
                    await context.Accounts.AddAsync(account);
                }
            }
            await context.SaveChangesAsync();
        }

        var countAfterFirstSeed = 0;

        // Second restart - attempt to seed again
        using (var context = new ApplicationDbContext(
            new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options))
        {
            countAfterFirstSeed = await context.Accounts.CountAsync();

            foreach (var account in seedAccounts)
            {
                if (!await context.Accounts.AnyAsync(a => a.Id == account.Id))
                {
                    await context.Accounts.AddAsync(account);
                }
            }
            await context.SaveChangesAsync();
        }

        // Third restart - verify no duplicates
        using (var context = new ApplicationDbContext(
            new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options))
        {
            var finalCount = await context.Accounts.CountAsync();

            Assert.Equal(2, countAfterFirstSeed);
            Assert.Equal(2, finalCount);
        }
    }
}
