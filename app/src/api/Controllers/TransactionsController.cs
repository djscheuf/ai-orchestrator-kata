using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FinancialApp.API.DTOs;

namespace FinancialApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TransactionsController : ControllerBase
{
    /// <summary>
    /// Get all transactions for the authenticated account.
    /// Returns transactions sorted by date descending.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TransactionDto>>> GetTransactions()
    {
        // Mock data - return sample transactions
        var mockTransactions = new[]
        {
            new TransactionDto(
                Id: "550e8400-e29b-41d4-a716-446655440101",
                SourceAccountId: "550e8400-e29b-41d4-a716-446655440001",
                TargetAccountId: "550e8400-e29b-41d4-a716-446655440002",
                Amount: 50.00m,
                Date: "2026-06-09",
                CreatedAt: "2026-06-09T10:30:00Z"
            ),
            new TransactionDto(
                Id: "550e8400-e29b-41d4-a716-446655440102",
                SourceAccountId: "550e8400-e29b-41d4-a716-446655440002",
                TargetAccountId: "550e8400-e29b-41d4-a716-446655440001",
                Amount: 75.50m,
                Date: "2026-06-08",
                CreatedAt: "2026-06-08T14:15:00Z"
            ),
            new TransactionDto(
                Id: "550e8400-e29b-41d4-a716-446655440103",
                SourceAccountId: "550e8400-e29b-41d4-a716-446655440001",
                TargetAccountId: "550e8400-e29b-41d4-a716-446655440003",
                Amount: 100.00m,
                Date: "2026-06-07",
                CreatedAt: "2026-06-07T09:45:00Z"
            ),
            new TransactionDto(
                Id: "550e8400-e29b-41d4-a716-446655440104",
                SourceAccountId: "550e8400-e29b-41d4-a716-446655440003",
                TargetAccountId: "550e8400-e29b-41d4-a716-446655440002",
                Amount: 125.75m,
                Date: "2026-06-06",
                CreatedAt: "2026-06-06T11:20:00Z"
            )
        };
        
        return Ok(mockTransactions);
    }
    
    /// <summary>
    /// Create a new transaction from the authenticated account.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<TransactionDto>> CreateTransaction([FromBody] CreateTransactionRequest request)
    {
        // Validate input
        if (request.Amount <= 0)
        {
            return BadRequest(new { message = "Amount must be positive" });
        }
        
        // Mock transaction creation
        var newTransaction = new TransactionDto(
            Id: Guid.NewGuid().ToString(),
            SourceAccountId: "550e8400-e29b-41d4-a716-446655440001", // Mock authenticated account
            TargetAccountId: request.TargetAccountId,
            Amount: request.Amount,
            Date: request.Date,
            CreatedAt: DateTime.UtcNow.ToString("O")
        );
        
        return CreatedAtAction(nameof(GetTransactions), newTransaction);
    }
}
