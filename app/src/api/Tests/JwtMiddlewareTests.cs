using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;
using FinancialApp.API.Middleware;
using FinancialApp.API.Services;

namespace FinancialApp.API.Tests;

public class JwtMiddlewareTests
{
    private readonly Mock<IJwtTokenService> _mockJwtTokenService;
    private readonly JwtMiddleware _middleware;
    
    public JwtMiddlewareTests()
    {
        _mockJwtTokenService = new Mock<IJwtTokenService>();
        _middleware = new JwtMiddleware(
            next: context => Task.CompletedTask,
            jwtTokenService: _mockJwtTokenService.Object
        );
    }
    
    [Fact]
    public async Task InvokeAsync_WithValidTokenInAuthorizationHeader_ValidatesToken()
    {
        // Arrange
        var validToken = "valid.jwt.token";
        var accountId = "550e8400-e29b-41d4-a716-446655440001";
        var claims = new List<Claim>
        {
            new Claim("accountId", accountId)
        };
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims));
        
        _mockJwtTokenService
            .Setup(s => s.ValidateToken(validToken))
            .Returns(claimsPrincipal);
        
        var context = new DefaultHttpContext();
        context.Request.Headers["Authorization"] = $"Bearer {validToken}";
        
        // Act
        await _middleware.InvokeAsync(context);
        
        // Assert
        _mockJwtTokenService.Verify(s => s.ValidateToken(validToken), Times.Once);
        Assert.NotNull(context.User);
        Assert.Equal(accountId, context.User.FindFirst("accountId")?.Value);
    }
    
    [Fact]
    public async Task InvokeAsync_WithInvalidToken_DoesNotSetUser()
    {
        // Arrange
        var invalidToken = "invalid.jwt.token";
        
        _mockJwtTokenService
            .Setup(s => s.ValidateToken(invalidToken))
            .Returns((ClaimsPrincipal)null);
        
        var context = new DefaultHttpContext();
        context.Request.Headers["Authorization"] = $"Bearer {invalidToken}";
        
        // Act
        await _middleware.InvokeAsync(context);
        
        // Assert
        _mockJwtTokenService.Verify(s => s.ValidateToken(invalidToken), Times.Once);
        Assert.Null(context.User.Identity?.Name);
    }
    
    [Fact]
    public async Task InvokeAsync_WithMissingAuthorizationHeader_DoesNotValidateToken()
    {
        // Arrange
        var context = new DefaultHttpContext();
        // No Authorization header set
        
        // Act
        await _middleware.InvokeAsync(context);
        
        // Assert
        _mockJwtTokenService.Verify(s => s.ValidateToken(It.IsAny<string>()), Times.Never);
    }
    
    [Fact]
    public async Task InvokeAsync_WithMissingBearerScheme_DoesNotValidateToken()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers["Authorization"] = "Basic sometoken";
        
        // Act
        await _middleware.InvokeAsync(context);
        
        // Assert
        _mockJwtTokenService.Verify(s => s.ValidateToken(It.IsAny<string>()), Times.Never);
    }
    
    [Fact]
    public async Task InvokeAsync_WithValidToken_SetsUserPrincipal()
    {
        // Arrange
        var validToken = "valid.jwt.token";
        var accountId = "550e8400-e29b-41d4-a716-446655440001";
        var claims = new List<Claim>
        {
            new Claim("accountId", accountId)
        };
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer"));
        
        _mockJwtTokenService
            .Setup(s => s.ValidateToken(validToken))
            .Returns(claimsPrincipal);
        
        var context = new DefaultHttpContext();
        context.Request.Headers["Authorization"] = $"Bearer {validToken}";
        
        // Act
        await _middleware.InvokeAsync(context);
        
        // Assert
        Assert.NotNull(context.User);
        Assert.True(context.User.Identity?.IsAuthenticated);
    }
}
