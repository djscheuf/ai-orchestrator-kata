using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using FinancialApp.API.DTOs;
using FinancialApp.API.Repositories;
using FinancialApp.API.Services;

namespace FinancialApp.API.Tests;

public class TransactionEndpointIntegrationTests
{
    private readonly InMemoryTransactionRepository _transactionRepository;
    private readonly InMemoryAccountRepository _accountRepository;
    private readonly IBalanceCalculationService _balanceCalculationService;
    private readonly ITransactionValidationService _validationService;
    
    public TransactionEndpointIntegrationTests()
    {
        _transactionRepository = new InMemoryTransactionRepository();
        _accountRepository = new InMemoryAccountRepository(_transactionRepository);
        _balanceCalculationService = new BalanceCalculationService(_transactionRepository);
        _validationService = new TransactionValidationService(_accountRepository);
        
        SetupTestData();
    }
    
    private void SetupTestData()
    {
        // Create test accounts
        _accountRepository.AddAccount(new Account
        {
            Id = "550e8400-e29b-41d4-a716-446655440001",
            AccountName = "Alice Account",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        
        _accountRepository.AddAccount(new Account
        {
            Id = "550e8400-e29b-41d4-a716-446655440002",
            AccountName = "Bob Account",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        
        // Create initial transactions to give Alice some balance
        _transactionRepository.CreateAsync(new Transaction
        {
            Id = Guid.NewGuid().ToString(),
            SourceAccountId = "550e8400-e29b-41d4-a716-446655440002",
            TargetAccountId = "550e8400-e29b-41d4-a716-446655440001",
            Amount = 500m,
            TransactionDate = DateTime.UtcNow.Date.AddDays(-1),
            CreatedAt = DateTime.UtcNow
        }).Wait();
    }
    
    [Fact]
    public async Task GetTransactionsForAccount_ReturnsAllTransactionsForAccount()
    {
        // Arrange
        var accountId = "550e8400-e29b-41d4-a716-446655440001";
        
        // Act
        var transactions = await _transactionRepository.GetByAccountIdAsync(accountId);
        
        // Assert
        Assert.NotNull(transactions);
        Assert.Single(transactions);
        var transaction = transactions.First();
        Assert.Equal(accountId, transaction.TargetAccountId);
    }
    
    [Fact]
    public async Task GetTransactionsForAccount_ReturnsSortedByDateDescending()
    {
        // Arrange
        var accountId = "550e8400-e29b-41d4-a716-446655440001";
        
        // Add multiple transactions
        await _transactionRepository.CreateAsync(new Transaction
        {
            Id = Guid.NewGuid().ToString(),
            SourceAccountId = "550e8400-e29b-41d4-a716-446655440002",
            TargetAccountId = accountId,
            Amount = 100m,
            TransactionDate = DateTime.UtcNow.Date,
            CreatedAt = DateTime.UtcNow
        });
        
        await _transactionRepository.CreateAsync(new Transaction
        {
            Id = Guid.NewGuid().ToString(),
            SourceAccountId = "550e8400-e29b-41d4-a716-446655440002",
            TargetAccountId = accountId,
            Amount = 200m,
            TransactionDate = DateTime.UtcNow.Date.AddDays(-2),
            CreatedAt = DateTime.UtcNow
        });
        
        // Act
        var transactions = await _transactionRepository.GetByAccountIdAsync(accountId);
        var transactionList = transactions.ToList();
        
        // Assert
        Assert.Equal(3, transactionList.Count);
        // Most recent should be first
        Assert.True(transactionList[0].TransactionDate >= transactionList[1].TransactionDate);
        Assert.True(transactionList[1].TransactionDate >= transactionList[2].TransactionDate);
    }
    
    [Fact]
    public async Task GetTransactionsForAccount_OnlyReturnsTransactionsForSpecificAccount()
    {
        // Arrange
        var aliceId = "550e8400-e29b-41d4-a716-446655440001";
        var bobId = "550e8400-e29b-41d4-a716-446655440002";
        var charlieId = "550e8400-e29b-41d4-a716-446655440003";
        
        // Add account for Charlie
        _accountRepository.AddAccount(new Account
        {
            Id = charlieId,
            AccountName = "Charlie Account",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        
        // Add transaction between Bob and Charlie (not involving Alice)
        await _transactionRepository.CreateAsync(new Transaction
        {
            Id = Guid.NewGuid().ToString(),
            SourceAccountId = bobId,
            TargetAccountId = charlieId,
            Amount = 100m,
            TransactionDate = DateTime.UtcNow.Date,
            CreatedAt = DateTime.UtcNow
        });
        
        // Act
        var aliceTransactions = await _transactionRepository.GetByAccountIdAsync(aliceId);
        var charlieTransactions = await _transactionRepository.GetByAccountIdAsync(charlieId);
        
        // Assert
        Assert.DoesNotContain(aliceTransactions, t => t.SourceAccountId == bobId && t.TargetAccountId == charlieId);
        Assert.Single(charlieTransactions.Where(t => t.SourceAccountId == bobId && t.TargetAccountId == charlieId));
    }
    
    [Fact]
    public async Task CreateTransaction_WithValidData_CreatesTransaction()
    {
        // Arrange
        var sourceAccountId = "550e8400-e29b-41d4-a716-446655440001";
        var targetAccountId = "550e8400-e29b-41d4-a716-446655440002";
        var amount = 100m;
        var transactionDate = DateTime.UtcNow.Date;
        
        // Validate
        var validationResult = await _validationService.ValidateTransactionAsync(
            sourceAccountId, targetAccountId, amount, transactionDate);
        Assert.True(validationResult.IsValid);
        
        // Act
        var transaction = await _transactionRepository.CreateAsync(new Transaction
        {
            SourceAccountId = sourceAccountId,
            TargetAccountId = targetAccountId,
            Amount = amount,
            TransactionDate = transactionDate,
            CreatedAt = DateTime.UtcNow
        });
        
        // Assert
        Assert.NotNull(transaction);
        Assert.NotEmpty(transaction.Id);
        Assert.Equal(sourceAccountId, transaction.SourceAccountId);
        Assert.Equal(targetAccountId, transaction.TargetAccountId);
        Assert.Equal(amount, transaction.Amount);
    }
    
    [Fact]
    public async Task CreateTransaction_UpdatesBalance()
    {
        // Arrange
        var sourceAccountId = "550e8400-e29b-41d4-a716-446655440001";
        var targetAccountId = "550e8400-e29b-41d4-a716-446655440002";
        var amount = 100m;
        
        var balanceBefore = await _balanceCalculationService.CalculateBalanceAsync(sourceAccountId);
        
        // Act
        await _transactionRepository.CreateAsync(new Transaction
        {
            SourceAccountId = sourceAccountId,
            TargetAccountId = targetAccountId,
            Amount = amount,
            TransactionDate = DateTime.UtcNow.Date,
            CreatedAt = DateTime.UtcNow
        });
        
        var balanceAfter = await _balanceCalculationService.CalculateBalanceAsync(sourceAccountId);
        
        // Assert
        Assert.Equal(balanceBefore - amount, balanceAfter);
    }
    
    [Fact]
    public async Task GetBalance_ReturnsCorrectBalance()
    {
        // Arrange
        var accountId = "550e8400-e29b-41d4-a716-446655440001";
        
        // Act
        var balance = await _balanceCalculationService.CalculateBalanceAsync(accountId);
        
        // Assert
        // Alice received 500 from Bob initially
        Assert.Equal(500m, balance);
    }
}
