using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FinancialApp.API.DTOs;

namespace FinancialApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AccountsController : ControllerBase
{
    /// <summary>
    /// Get current balance for the authenticated account.
    /// </summary>
    [HttpGet("balance")]
    public async Task<ActionResult<BalanceResponse>> GetBalance()
    {
        // Mock balance calculation
        var mockBalance = new BalanceResponse(
            AccountId: "550e8400-e29b-41d4-a716-446655440001",
            AccountName: "Alice Account",
            Balance: 1250.50m
        );
        
        return Ok(mockBalance);
    }
}
