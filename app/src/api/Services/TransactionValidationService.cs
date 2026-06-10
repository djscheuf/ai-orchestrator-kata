using System;
using System.Threading.Tasks;
using FinancialApp.API.Repositories;

namespace FinancialApp.API.Services;

/// <summary>
/// Validation result for transaction operations.
/// </summary>
public record ValidationResult(bool IsValid, string ErrorMessage = null);

/// <summary>
/// Service for validating transaction business rules.
/// </summary>
public interface ITransactionValidationService
{
    /// <summary>
    /// Validate a transaction creation request.
    /// </summary>
    Task<ValidationResult> ValidateTransactionAsync(
        string sourceAccountId,
        string targetAccountId,
        decimal amount,
        DateTime transactionDate);
}

public class TransactionValidationService : ITransactionValidationService
{
    private readonly IAccountRepository _accountRepository;
    
    public TransactionValidationService(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }
    
    public async Task<ValidationResult> ValidateTransactionAsync(
        string sourceAccountId,
        string targetAccountId,
        decimal amount,
        DateTime transactionDate)
    {
        // Validate non-zero amount
        if (amount <= 0)
        {
            return new ValidationResult(false, "Amount must be non-zero");
        }
        
        // Validate date is not in the future
        if (transactionDate > DateTime.UtcNow.Date)
        {
            return new ValidationResult(false, "Future dates not allowed");
        }
        
        // Validate source and target are different
        if (sourceAccountId == targetAccountId)
        {
            return new ValidationResult(false, "Source and target accounts must be different");
        }
        
        // Validate sufficient funds
        var balance = await _accountRepository.GetBalanceAsync(sourceAccountId);
        if (balance < amount)
        {
            return new ValidationResult(false, "Insufficient funds");
        }
        
        return new ValidationResult(true);
    }
}
