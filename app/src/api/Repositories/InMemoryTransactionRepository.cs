using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinancialApp.API.Repositories;

/// <summary>
/// In-memory implementation of transaction repository for testing.
/// </summary>
public class InMemoryTransactionRepository : ITransactionRepository
{
    private readonly List<Transaction> _transactions = new();
    
    public Task<IEnumerable<Transaction>> GetByAccountIdAsync(string accountId)
    {
        var transactions = _transactions
            .Where(t => t.SourceAccountId == accountId || t.TargetAccountId == accountId)
            .OrderByDescending(t => t.TransactionDate)
            .ThenByDescending(t => t.CreatedAt)
            .ToList();
        
        return Task.FromResult((IEnumerable<Transaction>)transactions);
    }
    
    public Task<IEnumerable<Transaction>> GetAllAsync()
    {
        return Task.FromResult((IEnumerable<Transaction>)_transactions.ToList());
    }
    
    public Task<Transaction> CreateAsync(Transaction transaction)
    {
        if (string.IsNullOrEmpty(transaction.Id))
        {
            transaction.Id = Guid.NewGuid().ToString();
        }
        
        if (transaction.CreatedAt == default)
        {
            transaction.CreatedAt = DateTime.UtcNow;
        }
        
        _transactions.Add(transaction);
        return Task.FromResult(transaction);
    }
    
    public void Clear()
    {
        _transactions.Clear();
    }
}
