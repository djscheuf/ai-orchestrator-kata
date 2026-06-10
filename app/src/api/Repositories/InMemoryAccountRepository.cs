using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinancialApp.API.Repositories;

/// <summary>
/// In-memory implementation of account repository for testing.
/// </summary>
public class InMemoryAccountRepository : IAccountRepository
{
    private readonly List<Account> _accounts = new();
    private readonly ITransactionRepository _transactionRepository;
    
    public InMemoryAccountRepository(ITransactionRepository transactionRepository = null)
    {
        _transactionRepository = transactionRepository;
    }
    
    public Task<Account> GetByIdAsync(string accountId)
    {
        var account = _accounts.FirstOrDefault(a => a.Id == accountId);
        return Task.FromResult(account);
    }
    
    public Task<IEnumerable<Account>> GetAllAsync()
    {
        return Task.FromResult((IEnumerable<Account>)_accounts.ToList());
    }
    
    public async Task<decimal> GetBalanceAsync(string accountId)
    {
        if (_transactionRepository == null)
        {
            return 0m;
        }
        
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
    
    public void AddAccount(Account account)
    {
        _accounts.Add(account);
    }
    
    public void Clear()
    {
        _accounts.Clear();
    }
}
