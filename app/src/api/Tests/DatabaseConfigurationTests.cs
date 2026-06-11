using System;
using Xunit;
using Microsoft.EntityFrameworkCore;
using FinancialApp.API.Data;
using FinancialApp.API.Repositories;

namespace FinancialApp.API.Tests;

public class DatabaseConfigurationTests
{
    [Fact]
    public void ApplicationDbContext_HasDbSetAccount_Property()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_" + Guid.NewGuid().ToString())
            .Options;

        // Act
        using (var context = new ApplicationDbContext(options))
        {
            // Assert
            var dbSetProperty = typeof(ApplicationDbContext).GetProperty("Accounts");
            Assert.NotNull(dbSetProperty);
            Assert.Equal(typeof(DbSet<Account>), dbSetProperty.PropertyType);
        }
    }

    [Fact]
    public void ApplicationDbContext_HasDbSetTransaction_Property()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_" + Guid.NewGuid().ToString())
            .Options;

        // Act
        using (var context = new ApplicationDbContext(options))
        {
            // Assert
            var dbSetProperty = typeof(ApplicationDbContext).GetProperty("Transactions");
            Assert.NotNull(dbSetProperty);
            Assert.Equal(typeof(DbSet<Transaction>), dbSetProperty.PropertyType);
        }
    }

    [Fact]
    public void ApplicationDbContext_ConfiguresAccountEntity_InOnModelCreating()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_" + Guid.NewGuid().ToString())
            .Options;

        // Act
        using (var context = new ApplicationDbContext(options))
        {
            var model = context.Model;
            var accountEntity = model.FindEntityType(typeof(Account));

            // Assert
            Assert.NotNull(accountEntity);
            Assert.NotNull(accountEntity.FindProperty("Id"));
            Assert.NotNull(accountEntity.FindProperty("AccountName"));
            Assert.NotNull(accountEntity.FindProperty("CreatedAt"));
            Assert.NotNull(accountEntity.FindProperty("UpdatedAt"));
        }
    }

    [Fact]
    public void ApplicationDbContext_ConfiguresTransactionEntity_InOnModelCreating()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_" + Guid.NewGuid().ToString())
            .Options;

        // Act
        using (var context = new ApplicationDbContext(options))
        {
            var model = context.Model;
            var transactionEntity = model.FindEntityType(typeof(Transaction));

            // Assert
            Assert.NotNull(transactionEntity);
            Assert.NotNull(transactionEntity.FindProperty("Id"));
            Assert.NotNull(transactionEntity.FindProperty("SourceAccountId"));
            Assert.NotNull(transactionEntity.FindProperty("TargetAccountId"));
            Assert.NotNull(transactionEntity.FindProperty("Amount"));
            Assert.NotNull(transactionEntity.FindProperty("TransactionDate"));
            Assert.NotNull(transactionEntity.FindProperty("CreatedAt"));
        }
    }
}
