using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using FinancialApp.API.Data;
using FinancialApp.API.Repositories;

namespace FinancialApp.API.Tests;

public class SeedDataIdempotencyTests : IAsyncLifetime
{
    private ApplicationDbContext _context;

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        await _context.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
    }

    [Fact]
    public async Task SeedData_WhenRunTwice_DoesNotCreateDuplicateAccounts()
    {
        // Arrange
        var seedAccounts = new[]
        {
            new Account 
            { 
                Id = "550e8400-e29b-41d4-a716-446655440001", 
                AccountName = "Alice Account",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Account 
            { 
                Id = "550e8400-e29b-41d4-a716-446655440002", 
                AccountName = "Bob Account",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        // Act - First seed
        foreach (var account in seedAccounts)
        {
            if (!_context.Accounts.Any(a => a.Id == account.Id))
            {
                _context.Accounts.Add(account);
            }
        }
        await _context.SaveChangesAsync();

        var countAfterFirstSeed = _context.Accounts.Count();

        // Act - Second seed (simulating application restart)
        foreach (var account in seedAccounts)
        {
            if (!_context.Accounts.Any(a => a.Id == account.Id))
            {
                _context.Accounts.Add(account);
            }
        }
        await _context.SaveChangesAsync();

        var countAfterSecondSeed = _context.Accounts.Count();

        // Assert
        Assert.Equal(2, countAfterFirstSeed);
        Assert.Equal(2, countAfterSecondSeed);
        Assert.Equal(countAfterFirstSeed, countAfterSecondSeed);
    }

    [Fact]
    public async Task SeedData_WithExistingAccounts_PreservesExistingData()
    {
        // Arrange
        var existingAccount = new Account 
        { 
            Id = "550e8400-e29b-41d4-a716-446655440001", 
            AccountName = "Alice Account",
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };

        _context.Accounts.Add(existingAccount);
        await _context.SaveChangesAsync();

        var originalCreatedAt = existingAccount.CreatedAt;

        // Act - Seed data with same ID but different name
        var seedAccounts = new[]
        {
            new Account 
            { 
                Id = "550e8400-e29b-41d4-a716-446655440001", 
                AccountName = "Alice Account Updated",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        foreach (var account in seedAccounts)
        {
            if (!_context.Accounts.Any(a => a.Id == account.Id))
            {
                _context.Accounts.Add(account);
            }
        }
        await _context.SaveChangesAsync();

        // Assert
        var savedAccount = _context.Accounts.First(a => a.Id == "550e8400-e29b-41d4-a716-446655440001");
        Assert.Equal("Alice Account", savedAccount.AccountName);
        Assert.Equal(originalCreatedAt, savedAccount.CreatedAt);
    }

    [Fact]
    public async Task SeedData_CanBeAddedIncrementally()
    {
        // Arrange
        var firstBatch = new[]
        {
            new Account 
            { 
                Id = "550e8400-e29b-41d4-a716-446655440001", 
                AccountName = "Alice Account",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        var secondBatch = new[]
        {
            new Account 
            { 
                Id = "550e8400-e29b-41d4-a716-446655440002", 
                AccountName = "Bob Account",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        // Act - Add first batch
        foreach (var account in firstBatch)
        {
            if (!_context.Accounts.Any(a => a.Id == account.Id))
            {
                _context.Accounts.Add(account);
            }
        }
        await _context.SaveChangesAsync();

        var countAfterFirstBatch = _context.Accounts.Count();

        // Act - Add second batch
        foreach (var account in secondBatch)
        {
            if (!_context.Accounts.Any(a => a.Id == account.Id))
            {
                _context.Accounts.Add(account);
            }
        }
        await _context.SaveChangesAsync();

        var countAfterSecondBatch = _context.Accounts.Count();

        // Assert
        Assert.Equal(1, countAfterFirstBatch);
        Assert.Equal(2, countAfterSecondBatch);
    }
}
