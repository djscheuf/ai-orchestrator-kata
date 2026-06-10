using System;
using System.IdentityModel.Tokens.Jwt;
using Xunit;
using FinancialApp.API.Services;

namespace FinancialApp.API.Tests;

public class JwtTokenServiceTests
{
    private readonly IJwtTokenService _jwtTokenService;
    
    public JwtTokenServiceTests()
    {
        _jwtTokenService = new JwtTokenService();
    }
    
    [Fact]
    public void GenerateToken_WithValidAccountId_ReturnsValidJwtToken()
    {
        // Arrange
        var accountId = "550e8400-e29b-41d4-a716-446655440001";
        
        // Act
        var token = _jwtTokenService.GenerateToken(accountId);
        
        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
        
        // Token should have three parts separated by dots (header.payload.signature)
        var parts = token.Split('.');
        Assert.Equal(3, parts.Length);
    }
    
    [Fact]
    public void GenerateToken_WithValidAccountId_TokenContainsAccountIdClaim()
    {
        // Arrange
        var accountId = "550e8400-e29b-41d4-a716-446655440001";
        
        // Act
        var token = _jwtTokenService.GenerateToken(accountId);
        
        // Assert
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;
        
        Assert.NotNull(jwtToken);
        var accountIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "accountId");
        Assert.NotNull(accountIdClaim);
        Assert.Equal(accountId, accountIdClaim.Value);
    }
    
    [Fact]
    public void GenerateToken_TokenHasExpirationClaim()
    {
        // Arrange
        var accountId = "550e8400-e29b-41d4-a716-446655440001";
        
        // Act
        var token = _jwtTokenService.GenerateToken(accountId);
        
        // Assert
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;
        
        Assert.NotNull(jwtToken);
        Assert.NotNull(jwtToken.ValidTo);
        Assert.True(jwtToken.ValidTo > DateTime.UtcNow);
    }
    
    [Fact]
    public void ValidateToken_WithValidToken_ReturnsClaimsPrincipal()
    {
        // Arrange
        var accountId = "550e8400-e29b-41d4-a716-446655440001";
        var token = _jwtTokenService.GenerateToken(accountId);
        
        // Act
        var principal = _jwtTokenService.ValidateToken(token);
        
        // Assert
        Assert.NotNull(principal);
        var accountIdClaim = principal.Claims.FirstOrDefault(c => c.Type == "accountId");
        Assert.NotNull(accountIdClaim);
        Assert.Equal(accountId, accountIdClaim.Value);
    }
    
    [Fact]
    public void ValidateToken_WithInvalidToken_ReturnsNull()
    {
        // Arrange
        var invalidToken = "invalid.token.here";
        
        // Act
        var principal = _jwtTokenService.ValidateToken(invalidToken);
        
        // Assert
        Assert.Null(principal);
    }
    
    [Fact]
    public void ValidateToken_WithTamperedToken_ReturnsNull()
    {
        // Arrange
        var accountId = "550e8400-e29b-41d4-a716-446655440001";
        var token = _jwtTokenService.GenerateToken(accountId);
        var parts = token.Split('.');
        var tamperedToken = parts[0] + "." + parts[1] + ".invalidsignature";
        
        // Act
        var principal = _jwtTokenService.ValidateToken(tamperedToken);
        
        // Assert
        Assert.Null(principal);
    }
}
