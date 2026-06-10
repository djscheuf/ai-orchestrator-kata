using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FinancialApp.API.DTOs;
using FinancialApp.API.Repositories;
using FinancialApp.API.Services;

namespace FinancialApp.API.Controllers;

[ApiController]
[Route("api/accounts/{accountId}/[controller]")]
[Authorize]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ITransactionValidationService _validationService;
    private readonly IBalanceCalculationService _balanceCalculationService;
    
    public TransactionsController(
        ITransactionRepository transactionRepository,
        IAccountRepository accountRepository,
        ITransactionValidationService validationService,
        IBalanceCalculationService balanceCalculationService)
    {
        _transactionRepository = transactionRepository;
        _accountRepository = accountRepository;
        _validationService = validationService;
        _balanceCalculationService = balanceCalculationService;
    }
    
    /// <summary>
    /// Get all transactions for the authenticated account.
    /// Returns transactions sorted by date descending.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<TransactionListResponse>> GetTransactions(string accountId)
    {
        // Verify account exists
        var account = await _accountRepository.GetByIdAsync(accountId);
        if (account == null)
        {
            return NotFound(new { error = "Account not found" });
        }
        
        // Get transactions
        var transactions = await _transactionRepository.GetByAccountIdAsync(accountId);
        var transactionDtos = transactions.Select(t => new TransactionDto(
            Id: t.Id,
            SourceAccountId: t.SourceAccountId,
            TargetAccountId: t.TargetAccountId,
            Amount: t.Amount,
            Date: t.TransactionDate.ToString("yyyy-MM-dd"),
            CreatedAt: t.CreatedAt.ToString("O")
        )).ToList();
        
        // Calculate balance
        var balance = await _balanceCalculationService.CalculateBalanceAsync(accountId);
        
        var response = new TransactionListResponse(
            Transactions: transactionDtos,
            Balance: balance
        );
        
        return Ok(response);
    }
    
    /// <summary>
    /// Create a new transaction from the authenticated account.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<TransactionDto>> CreateTransaction(
        string accountId,
        [FromBody] CreateTransactionRequest request)
    {
        // Validate input
        if (request == null)
        {
            return BadRequest(new { error = "Request body is required" });
        }
        
        if (string.IsNullOrEmpty(request.TargetAccountId))
        {
            return BadRequest(new { error = "Target account is required" });
        }
        
        // Parse transaction date
        if (!DateTime.TryParse(request.Date, out var transactionDate))
        {
            return BadRequest(new { error = "Invalid date format" });
        }
        
        // Validate transaction
        var validationResult = await _validationService.ValidateTransactionAsync(
            accountId,
            request.TargetAccountId,
            request.Amount,
            transactionDate);
        
        if (!validationResult.IsValid)
        {
            if (validationResult.ErrorMessage.Contains("Insufficient"))
            {
                return StatusCode(422, new { error = validationResult.ErrorMessage });
            }
            return BadRequest(new { error = validationResult.ErrorMessage });
        }
        
        // Create transaction
        var transaction = new Transaction
        {
            Id = Guid.NewGuid().ToString(),
            SourceAccountId = accountId,
            TargetAccountId = request.TargetAccountId,
            Amount = request.Amount,
            TransactionDate = transactionDate,
            CreatedAt = DateTime.UtcNow
        };
        
        var createdTransaction = await _transactionRepository.CreateAsync(transaction);
        
        var response = new TransactionDto(
            Id: createdTransaction.Id,
            SourceAccountId: createdTransaction.SourceAccountId,
            TargetAccountId: createdTransaction.TargetAccountId,
            Amount: createdTransaction.Amount,
            Date: createdTransaction.TransactionDate.ToString("yyyy-MM-dd"),
            CreatedAt: createdTransaction.CreatedAt.ToString("O")
        );
        
        return CreatedAtAction(nameof(GetTransactions), new { accountId }, response);
    }
}
