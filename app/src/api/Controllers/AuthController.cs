using Microsoft.AspNetCore.Mvc;
using FinancialApp.API.DTOs;

namespace FinancialApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    /// <summary>
    /// Authenticate user with username and password.
    /// Returns JWT token and account information.
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        // Mock authentication - accept any valid username from seed data
        var validUsernames = new[] { "user_a", "user_b", "user_c", "user_d", "user_e" };
        
        if (!validUsernames.Contains(request.Username))
        {
            return Unauthorized(new { message = "Invalid credentials" });
        }
        
        // Mock token generation
        var mockToken = $"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.{request.Username}.mock_signature";
        
        // Map username to account
        var accountMap = new Dictionary<string, (string Id, string Name)>
        {
            { "user_a", ("550e8400-e29b-41d4-a716-446655440001", "Alice Account") },
            { "user_b", ("550e8400-e29b-41d4-a716-446655440002", "Bob Account") },
            { "user_c", ("550e8400-e29b-41d4-a716-446655440003", "Charlie Account") },
            { "user_d", ("550e8400-e29b-41d4-a716-446655440004", "Diana Account") },
            { "user_e", ("550e8400-e29b-41d4-a716-446655440005", "Eve Account") }
        };
        
        var (accountId, accountName) = accountMap[request.Username];
        
        var response = new LoginResponse(
            Token: mockToken,
            AccountId: accountId,
            AccountName: accountName
        );
        
        return Ok(response);
    }
}
