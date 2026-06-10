using Microsoft.AspNetCore.Mvc;
using FinancialApp.API.DTOs;
using FinancialApp.API.Services;

namespace FinancialApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IAccountSecurityService _accountSecurityService;
    
    public AuthController(IJwtTokenService jwtTokenService, IAccountSecurityService accountSecurityService)
    {
        _jwtTokenService = jwtTokenService;
        _accountSecurityService = accountSecurityService;
    }
    
    /// <summary>
    /// Authenticate user with username and password.
    /// Returns JWT token and account information.
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        // Validate request
        if (string.IsNullOrWhiteSpace(request?.Username) || string.IsNullOrWhiteSpace(request?.Password))
        {
            return BadRequest(new { error = "Username and password are required" });
        }
        
        // Validate credentials
        var accountId = await _accountSecurityService.ValidateCredentialsAsync(request.Username, request.Password);
        if (accountId == null)
        {
            return Unauthorized(new { error = "Invalid credentials" });
        }
        
        // Get account information
        var accountInfo = await _accountSecurityService.GetAccountInfoAsync(accountId);
        if (accountInfo == null)
        {
            return Unauthorized(new { error = "Invalid credentials" });
        }
        
        // Generate token
        var token = _jwtTokenService.GenerateToken(accountId);
        
        var response = new LoginResponse(
            Token: token,
            AccountId: accountId,
            AccountName: accountInfo.AccountName
        );
        
        return Ok(response);
    }
}
