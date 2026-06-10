using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Xunit;
using FinancialApp.API.Services;
using FinancialApp.API.Repositories;

namespace FinancialApp.API.Tests;

public class BalanceCalculationServiceTests
{
    private readonly Mock<ITransactionRepository> _mockTransactionRepository;
    private readonly BalanceCalculationService _balanceCalculationService;
    
    public BalanceCalculationServiceTests()
    {
        _mockTransactionRepository = new Mock<ITransactionRepository>();
        _balanceCalculationService = new BalanceCalculationService(_mockTransactionRepository.Object);
    }
    
    [Fact]
    public async Task CalculateBalanceAsync_WithNoTransactions_ReturnsZero()
    {
        // Arrange
        var accountId = "550e8400-e29b-41d4-a716-446655440001";
        _mockTransactionRepository
            .Setup(r => r.GetByAccountIdAsync(accountId))
            .ReturnsAsync(new List<Transaction>());
        
        // Act
        var balance = await _balanceCalculationService.CalculateBalanceAsync(accountId);
        
        // Assert
        Assert.Equal(0m, balance);
    }
    
    [Fact]
    public async Task CalculateBalanceAsync_WithIncomingTransactions_ReturnsCorrectBalance()
    {
        // Arrange
        var accountId = "550e8400-e29b-41d4-a716-446655440001";
        var transactions = new List<Transaction>
        {
            new Transaction
            {
                Id = "1",
                SourceAccountId = "550e8400-e29b-41d4-a716-446655440002",
                TargetAccountId = accountId,
                Amount = 100m,
                TransactionDate = DateTime.UtcNow.Date,
                CreatedAt = DateTime.UtcNow
            },
            new Transaction
            {
                Id = "2",
                SourceAccountId = "550e8400-e29b-41d4-a716-446655440003",
                TargetAccountId = accountId,
                Amount = 50m,
                TransactionDate = DateTime.UtcNow.Date,
                CreatedAt = DateTime.UtcNow
            }
        };
        
        _mockTransactionRepository
            .Setup(r => r.GetByAccountIdAsync(accountId))
            .ReturnsAsync(transactions);
        
        // Act
        var balance = await _balanceCalculationService.CalculateBalanceAsync(accountId);
        
        // Assert
        Assert.Equal(150m, balance);
    }
    
    [Fact]
    public async Task CalculateBalanceAsync_WithOutgoingTransactions_ReturnsCorrectBalance()
    {
        // Arrange
        var accountId = "550e8400-e29b-41d4-a716-446655440001";
        var transactions = new List<Transaction>
        {
            new Transaction
            {
                Id = "1",
                SourceAccountId = accountId,
                TargetAccountId = "550e8400-e29b-41d4-a716-446655440002",
                Amount = 100m,
                TransactionDate = DateTime.UtcNow.Date,
                CreatedAt = DateTime.UtcNow
            },
            new Transaction
            {
                Id = "2",
                SourceAccountId = accountId,
                TargetAccountId = "550e8400-e29b-41d4-a716-446655440003",
                Amount = 50m,
                TransactionDate = DateTime.UtcNow.Date,
                CreatedAt = DateTime.UtcNow
            }
        };
        
        _mockTransactionRepository
            .Setup(r => r.GetByAccountIdAsync(accountId))
            .ReturnsAsync(transactions);
        
        // Act
        var balance = await _balanceCalculationService.CalculateBalanceAsync(accountId);
        
        // Assert
        Assert.Equal(-150m, balance);
    }
    
    [Fact]
    public async Task CalculateBalanceAsync_WithMixedTransactions_ReturnsCorrectBalance()
    {
        // Arrange
        var accountId = "550e8400-e29b-41d4-a716-446655440001";
        var transactions = new List<Transaction>
        {
            new Transaction
            {
                Id = "1",
                SourceAccountId = "550e8400-e29b-41d4-a716-446655440002",
                TargetAccountId = accountId,
                Amount = 200m,
                TransactionDate = DateTime.UtcNow.Date,
                CreatedAt = DateTime.UtcNow
            },
            new Transaction
            {
                Id = "2",
                SourceAccountId = accountId,
                TargetAccountId = "550e8400-e29b-41d4-a716-446655440003",
                Amount = 75m,
                TransactionDate = DateTime.UtcNow.Date,
                CreatedAt = DateTime.UtcNow
            },
            new Transaction
            {
                Id = "3",
                SourceAccountId = "550e8400-e29b-41d4-a716-446655440004",
                TargetAccountId = accountId,
                Amount = 50m,
                TransactionDate = DateTime.UtcNow.Date,
                CreatedAt = DateTime.UtcNow
            }
        };
        
        _mockTransactionRepository
            .Setup(r => r.GetByAccountIdAsync(accountId))
            .ReturnsAsync(transactions);
        
        // Act
        var balance = await _balanceCalculationService.CalculateBalanceAsync(accountId);
        
        // Assert
        // Incoming: 200 + 50 = 250
        // Outgoing: 75
        // Balance: 250 - 75 = 175
        Assert.Equal(175m, balance);
    }
    
    [Fact]
    public async Task CalculateBalanceAsync_WithNullTransactions_ReturnsZero()
    {
        // Arrange
        var accountId = "550e8400-e29b-41d4-a716-446655440001";
        _mockTransactionRepository
            .Setup(r => r.GetByAccountIdAsync(accountId))
            .ReturnsAsync((IEnumerable<Transaction>)null);
        
        // Act
        var balance = await _balanceCalculationService.CalculateBalanceAsync(accountId);
        
        // Assert
        Assert.Equal(0m, balance);
    }
}
