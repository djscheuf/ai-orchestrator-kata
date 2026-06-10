using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using FinancialApp.API.Controllers;
using FinancialApp.API.DTOs;
using FinancialApp.API.Services;

namespace FinancialApp.API.Tests;

public class AuthControllerTests
{
    private readonly Mock<IJwtTokenService> _mockJwtTokenService;
    private readonly Mock<IAccountSecurityService> _mockAccountSecurityService;
    private readonly AuthController _controller;
    
    public AuthControllerTests()
    {
        _mockJwtTokenService = new Mock<IJwtTokenService>();
        _mockAccountSecurityService = new Mock<IAccountSecurityService>();
        _controller = new AuthController(_mockJwtTokenService.Object, _mockAccountSecurityService.Object);
    }
    
    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOkWithToken()
    {
        // Arrange
        var request = new LoginRequest("user_a", "password");
        var accountId = "550e8400-e29b-41d4-a716-446655440001";
        var accountName = "Alice Account";
        var token = "valid.jwt.token";
        
        _mockAccountSecurityService
            .Setup(s => s.ValidateCredentialsAsync("user_a", "password"))
            .ReturnsAsync(accountId);
        
        _mockAccountSecurityService
            .Setup(s => s.GetAccountInfoAsync(accountId))
            .ReturnsAsync(new AccountInfo(accountId, accountName));
        
        _mockJwtTokenService
            .Setup(s => s.GenerateToken(accountId))
            .Returns(token);
        
        // Act
        var result = await _controller.Login(request);
        
        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<LoginResponse>(okResult.Value);
        Assert.Equal(token, response.Token);
        Assert.Equal(accountId, response.AccountId);
        Assert.Equal(accountName, response.AccountName);
    }
    
    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var request = new LoginRequest("invalid_user", "password");
        
        _mockAccountSecurityService
            .Setup(s => s.ValidateCredentialsAsync("invalid_user", "password"))
            .ReturnsAsync((string)null);
        
        // Act
        var result = await _controller.Login(request);
        
        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
        Assert.NotNull(unauthorizedResult.Value);
    }
    
    [Fact]
    public async Task Login_WithMissingUsername_ReturnsBadRequest()
    {
        // Arrange
        var request = new LoginRequest(null, "password");
        
        // Act
        var result = await _controller.Login(request);
        
        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.NotNull(badRequestResult.Value);
    }
    
    [Fact]
    public async Task Login_WithMissingPassword_ReturnsBadRequest()
    {
        // Arrange
        var request = new LoginRequest("user_a", null);
        
        // Act
        var result = await _controller.Login(request);
        
        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.NotNull(badRequestResult.Value);
    }
    
    [Fact]
    public async Task Login_WithEmptyUsername_ReturnsBadRequest()
    {
        // Arrange
        var request = new LoginRequest("", "password");
        
        // Act
        var result = await _controller.Login(request);
        
        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.NotNull(badRequestResult.Value);
    }
    
    [Fact]
    public async Task Login_WithValidCredentials_CallsJwtTokenService()
    {
        // Arrange
        var request = new LoginRequest("user_a", "password");
        var accountId = "550e8400-e29b-41d4-a716-446655440001";
        var accountName = "Alice Account";
        var token = "valid.jwt.token";
        
        _mockAccountSecurityService
            .Setup(s => s.ValidateCredentialsAsync("user_a", "password"))
            .ReturnsAsync(accountId);
        
        _mockAccountSecurityService
            .Setup(s => s.GetAccountInfoAsync(accountId))
            .ReturnsAsync(new AccountInfo(accountId, accountName));
        
        _mockJwtTokenService
            .Setup(s => s.GenerateToken(accountId))
            .Returns(token);
        
        // Act
        var result = await _controller.Login(request);
        
        // Assert
        _mockJwtTokenService.Verify(s => s.GenerateToken(accountId), Times.Once);
    }
}
