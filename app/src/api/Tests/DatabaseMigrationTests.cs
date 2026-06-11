using System;
using System.Linq;
using Xunit;
using Microsoft.EntityFrameworkCore;
using FinancialApp.API.Data;
using FinancialApp.API.Repositories;

namespace FinancialApp.API.Tests;

public class DatabaseMigrationTests
{
    [Fact]
    public void Database_CanBeCreatedWithMigrations()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_" + Guid.NewGuid().ToString())
            .Options;

        // Act
        using (var context = new ApplicationDbContext(options))
        {
            context.Database.EnsureCreated();
            
            // Assert
            Assert.True(context.Database.IsInMemory());
        }
    }

    [Fact]
    public void Database_CanInsertAccount_AfterMigration()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_" + Guid.NewGuid().ToString())
            .Options;

        using (var context = new ApplicationDbContext(options))
        {
            context.Database.EnsureCreated();
            
            var account = new Account
            {
                Id = Guid.NewGuid().ToString(),
                AccountName = "Test Account",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Act
            context.Accounts.Add(account);
            context.SaveChanges();

            // Assert
            var savedAccount = context.Accounts.FirstOrDefault(a => a.Id == account.Id);
            Assert.NotNull(savedAccount);
            Assert.Equal(account.AccountName, savedAccount.AccountName);
        }
    }

    [Fact]
    public void Database_CanInsertTransaction_AfterMigration()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_" + Guid.NewGuid().ToString())
            .Options;

        using (var context = new ApplicationDbContext(options))
        {
            context.Database.EnsureCreated();
            
            var transaction = new Transaction
            {
                Id = Guid.NewGuid().ToString(),
                SourceAccountId = Guid.NewGuid().ToString(),
                TargetAccountId = Guid.NewGuid().ToString(),
                Amount = 100m,
                TransactionDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            // Act
            context.Transactions.Add(transaction);
            context.SaveChanges();

            // Assert
            var savedTransaction = context.Transactions.FirstOrDefault(t => t.Id == transaction.Id);
            Assert.NotNull(savedTransaction);
            Assert.Equal(transaction.Amount, savedTransaction.Amount);
            Assert.Equal(transaction.SourceAccountId, savedTransaction.SourceAccountId);
            Assert.Equal(transaction.TargetAccountId, savedTransaction.TargetAccountId);
        }
    }

    [Fact]
    public void Database_ConfiguresAccountId_AsKeyProperty()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_" + Guid.NewGuid().ToString())
            .Options;

        using (var context = new ApplicationDbContext(options))
        {
            var model = context.Model;
            var accountEntity = model.FindEntityType(typeof(Account));

            // Act
            var keyProperties = accountEntity.FindPrimaryKey().Properties;

            // Assert
            Assert.Single(keyProperties);
            Assert.Equal("Id", keyProperties[0].Name);
        }
    }

    [Fact]
    public void Database_ConfiguresTransactionId_AsKeyProperty()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_" + Guid.NewGuid().ToString())
            .Options;

        using (var context = new ApplicationDbContext(options))
        {
            var model = context.Model;
            var transactionEntity = model.FindEntityType(typeof(Transaction));

            // Act
            var keyProperties = transactionEntity.FindPrimaryKey().Properties;

            // Assert
            Assert.Single(keyProperties);
            Assert.Equal("Id", keyProperties[0].Name);
        }
    }
}
