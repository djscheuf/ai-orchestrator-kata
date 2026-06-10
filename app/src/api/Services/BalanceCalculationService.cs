using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinancialApp.API.Repositories;

namespace FinancialApp.API.Services;

/// <summary>
/// Service for calculating account balances from transaction history.
/// </summary>
public interface IBalanceCalculationService
{
    /// <summary>
    /// Calculate balance for an account from its transaction history.
    /// Balance = SUM(incoming) - SUM(outgoing)
    /// </summary>
    Task<decimal> CalculateBalanceAsync(string accountId);
}

public class BalanceCalculationService : IBalanceCalculationService
{
    private readonly ITransactionRepository _transactionRepository;
    
    public BalanceCalculationService(ITransactionRepository transactionRepository)
    {
        _transactionRepository = transactionRepository;
    }
    
    public async Task<decimal> CalculateBalanceAsync(string accountId)
    {
        var transactions = await _transactionRepository.GetByAccountIdAsync(accountId);
        
        if (transactions == null || !transactions.Any())
        {
            return 0m;
        }
        
        var incoming = transactions
            .Where(t => t.TargetAccountId == accountId)
            .Sum(t => t.Amount);
        
        var outgoing = transactions
            .Where(t => t.SourceAccountId == accountId)
            .Sum(t => t.Amount);
        
        return incoming - outgoing;
    }
}
