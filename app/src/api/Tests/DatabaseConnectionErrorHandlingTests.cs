using System;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using FinancialApp.API.Data;
using FinancialApp.API.Repositories;

namespace FinancialApp.API.Tests;

public class DatabaseConnectionErrorHandlingTests
{
    [Fact]
    public void ApplicationDbContext_WithSqlServerOptions_CanBeConfigured()
    {
        // Arrange & Act
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer("Server=localhost;Database=TestDb;User Id=sa;Password=test;")
            .Options;

        // Assert - Configuration should succeed even if connection fails later
        Assert.NotNull(options);
        using (var context = new ApplicationDbContext(options))
        {
            Assert.NotNull(context);
        }
    }

    [Fact]
    public void ApplicationDbContext_CanBeCreatedWithValidInMemoryDatabase()
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
    public async Task SqlRepository_WithValidContext_CanPersistData()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_" + Guid.NewGuid().ToString())
            .Options;

        using (var context = new ApplicationDbContext(options))
        {
            context.Database.EnsureCreated();
            var repository = new SqlTransactionRepository(context);

            var transaction = new Transaction
            {
                Id = "txn-1",
                SourceAccountId = "account-1",
                TargetAccountId = "account-2",
                Amount = 100m,
                TransactionDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            // Act
            var result = await repository.CreateAsync(transaction);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("txn-1", result.Id);
        }
    }

    [Fact]
    public async Task SqlRepository_WhenContextIsDisposed_ThrowsException()
    {
        // Arrange
        ApplicationDbContext context;
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_" + Guid.NewGuid().ToString())
            .Options;

        using (context = new ApplicationDbContext(options))
        {
            context.Database.EnsureCreated();
        }

        var repository = new SqlTransactionRepository(context);

        var transaction = new Transaction
        {
            Id = "txn-1",
            SourceAccountId = "account-1",
            TargetAccountId = "account-2",
            Amount = 100m,
            TransactionDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        // Act & Assert
        await Assert.ThrowsAsync<ObjectDisposedException>(() => repository.CreateAsync(transaction));
    }

    [Fact]
    public async Task ApplicationDbContext_EnsureCreated_DoesNotThrowWhenDatabaseAlreadyExists()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_" + Guid.NewGuid().ToString())
            .Options;

        using (var context = new ApplicationDbContext(options))
        {
            // Act - First call
            context.Database.EnsureCreated();

            // Act - Second call (should not throw)
            var result = context.Database.EnsureCreated();

            // Assert
            Assert.False(result);
        }
    }
}
