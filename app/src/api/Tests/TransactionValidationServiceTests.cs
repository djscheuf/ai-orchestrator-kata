using System;
using System.Threading.Tasks;
using Moq;
using Xunit;
using FinancialApp.API.Services;
using FinancialApp.API.Repositories;

namespace FinancialApp.API.Tests;

public class TransactionValidationServiceTests
{
    private readonly Mock<IAccountRepository> _mockAccountRepository;
    private readonly TransactionValidationService _validationService;
    
    public TransactionValidationServiceTests()
    {
        _mockAccountRepository = new Mock<IAccountRepository>();
        _validationService = new TransactionValidationService(_mockAccountRepository.Object);
    }
    
    [Fact]
    public async Task ValidateTransactionAsync_WithZeroAmount_ReturnsFalseWithErrorMessage()
    {
        // Arrange
        var sourceAccountId = "550e8400-e29b-41d4-a716-446655440001";
        var targetAccountId = "550e8400-e29b-41d4-a716-446655440002";
        var amount = 0m;
        var transactionDate = DateTime.UtcNow.Date;
        
        // Act
        var result = await _validationService.ValidateTransactionAsync(
            sourceAccountId, targetAccountId, amount, transactionDate);
        
        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("Amount must be non-zero", result.ErrorMessage);
    }
    
    [Fact]
    public async Task ValidateTransactionAsync_WithNegativeAmount_ReturnsFalseWithErrorMessage()
    {
        // Arrange
        var sourceAccountId = "550e8400-e29b-41d4-a716-446655440001";
        var targetAccountId = "550e8400-e29b-41d4-a716-446655440002";
        var amount = -50m;
        var transactionDate = DateTime.UtcNow.Date;
        
        // Act
        var result = await _validationService.ValidateTransactionAsync(
            sourceAccountId, targetAccountId, amount, transactionDate);
        
        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("Amount must be non-zero", result.ErrorMessage);
    }
    
    [Fact]
    public async Task ValidateTransactionAsync_WithFutureDate_ReturnsFalseWithErrorMessage()
    {
        // Arrange
        var sourceAccountId = "550e8400-e29b-41d4-a716-446655440001";
        var targetAccountId = "550e8400-e29b-41d4-a716-446655440002";
        var amount = 50m;
        var transactionDate = DateTime.UtcNow.Date.AddDays(1);
        
        // Act
        var result = await _validationService.ValidateTransactionAsync(
            sourceAccountId, targetAccountId, amount, transactionDate);
        
        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("Future dates not allowed", result.ErrorMessage);
    }
    
    [Fact]
    public async Task ValidateTransactionAsync_WithInsufficientFunds_ReturnsFalseWithErrorMessage()
    {
        // Arrange
        var sourceAccountId = "550e8400-e29b-41d4-a716-446655440001";
        var targetAccountId = "550e8400-e29b-41d4-a716-446655440002";
        var amount = 150m;
        var transactionDate = DateTime.UtcNow.Date;
        
        _mockAccountRepository
            .Setup(r => r.GetBalanceAsync(sourceAccountId))
            .ReturnsAsync(100m);
        
        // Act
        var result = await _validationService.ValidateTransactionAsync(
            sourceAccountId, targetAccountId, amount, transactionDate);
        
        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("Insufficient funds", result.ErrorMessage);
    }
    
    [Fact]
    public async Task ValidateTransactionAsync_WithValidTransaction_ReturnsTrue()
    {
        // Arrange
        var sourceAccountId = "550e8400-e29b-41d4-a716-446655440001";
        var targetAccountId = "550e8400-e29b-41d4-a716-446655440002";
        var amount = 50m;
        var transactionDate = DateTime.UtcNow.Date;
        
        _mockAccountRepository
            .Setup(r => r.GetBalanceAsync(sourceAccountId))
            .ReturnsAsync(100m);
        
        // Act
        var result = await _validationService.ValidateTransactionAsync(
            sourceAccountId, targetAccountId, amount, transactionDate);
        
        // Assert
        Assert.True(result.IsValid);
        Assert.Null(result.ErrorMessage);
    }
    
    [Fact]
    public async Task ValidateTransactionAsync_WithExactBalance_ReturnsTrue()
    {
        // Arrange
        var sourceAccountId = "550e8400-e29b-41d4-a716-446655440001";
        var targetAccountId = "550e8400-e29b-41d4-a716-446655440002";
        var amount = 100m;
        var transactionDate = DateTime.UtcNow.Date;
        
        _mockAccountRepository
            .Setup(r => r.GetBalanceAsync(sourceAccountId))
            .ReturnsAsync(100m);
        
        // Act
        var result = await _validationService.ValidateTransactionAsync(
            sourceAccountId, targetAccountId, amount, transactionDate);
        
        // Assert
        Assert.True(result.IsValid);
    }
    
    [Fact]
    public async Task ValidateTransactionAsync_WithPastDate_ReturnsTrue()
    {
        // Arrange
        var sourceAccountId = "550e8400-e29b-41d4-a716-446655440001";
        var targetAccountId = "550e8400-e29b-41d4-a716-446655440002";
        var amount = 50m;
        var transactionDate = DateTime.UtcNow.Date.AddDays(-5);
        
        _mockAccountRepository
            .Setup(r => r.GetBalanceAsync(sourceAccountId))
            .ReturnsAsync(100m);
        
        // Act
        var result = await _validationService.ValidateTransactionAsync(
            sourceAccountId, targetAccountId, amount, transactionDate);
        
        // Assert
        Assert.True(result.IsValid);
    }
    
    [Fact]
    public async Task ValidateTransactionAsync_WithSameSourceAndTarget_ReturnsFalseWithErrorMessage()
    {
        // Arrange
        var accountId = "550e8400-e29b-41d4-a716-446655440001";
        var amount = 50m;
        var transactionDate = DateTime.UtcNow.Date;
        
        // Act
        var result = await _validationService.ValidateTransactionAsync(
            accountId, accountId, amount, transactionDate);
        
        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("Source and target accounts must be different", result.ErrorMessage);
    }
}
