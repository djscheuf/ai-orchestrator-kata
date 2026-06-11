using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FinancialApp.API.Data;

namespace FinancialApp.API.Repositories;

public class SqlAccountRepository : IAccountRepository
{
    private readonly ApplicationDbContext _context;

    public SqlAccountRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Account> GetByIdAsync(string accountId)
    {
        return await _context.Accounts.FindAsync(accountId);
    }

    public async Task<IEnumerable<Account>> GetAllAsync()
    {
        return await _context.Accounts.ToListAsync();
    }

    public async Task<decimal> GetBalanceAsync(string accountId)
    {
        var transactions = await _context.Transactions
            .Where(t => t.SourceAccountId == accountId || t.TargetAccountId == accountId)
            .ToListAsync();

        var balance = transactions
            .Where(t => t.TargetAccountId == accountId)
            .Sum(t => t.Amount) - 
            transactions
            .Where(t => t.SourceAccountId == accountId)
            .Sum(t => t.Amount);

        return balance;
    }
}
