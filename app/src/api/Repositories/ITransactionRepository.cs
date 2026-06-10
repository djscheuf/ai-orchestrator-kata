using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FinancialApp.API.Repositories;

/// <summary>
/// Transaction domain model.
/// </summary>
public class Transaction
{
    public string Id { get; set; }
    public string SourceAccountId { get; set; }
    public string TargetAccountId { get; set; }
    public decimal Amount { get; set; }
    public DateTime TransactionDate { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Repository interface for transaction data access.
/// </summary>
public interface ITransactionRepository
{
    /// <summary>
    /// Get all transactions for a specific account (as source or target).
    /// </summary>
    Task<IEnumerable<Transaction>> GetByAccountIdAsync(string accountId);
    
    /// <summary>
    /// Get all transactions in the system.
    /// </summary>
    Task<IEnumerable<Transaction>> GetAllAsync();
    
    /// <summary>
    /// Create a new transaction.
    /// </summary>
    Task<Transaction> CreateAsync(Transaction transaction);
}
