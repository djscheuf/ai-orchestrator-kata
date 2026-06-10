using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FinancialApp.API.Repositories;

/// <summary>
/// Account domain model.
/// </summary>
public class Account
{
    public string Id { get; set; }
    public string AccountName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Repository interface for account data access.
/// </summary>
public interface IAccountRepository
{
    /// <summary>
    /// Get account by ID.
    /// </summary>
    Task<Account> GetByIdAsync(string accountId);
    
    /// <summary>
    /// Get all accounts.
    /// </summary>
    Task<IEnumerable<Account>> GetAllAsync();
    
    /// <summary>
    /// Calculate balance for an account from its transactions.
    /// </summary>
    Task<decimal> GetBalanceAsync(string accountId);
}
