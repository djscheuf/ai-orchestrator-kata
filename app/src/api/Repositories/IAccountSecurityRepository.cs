using System;
using System.Threading.Tasks;

namespace FinancialApp.API.Repositories;

/// <summary>
/// Account security domain model.
/// </summary>
public class AccountSecurity
{
    public string SecurityId { get; set; }
    public string AccountId { get; set; }
    public string Username { get; set; }
    public string PasswordHash { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}

/// <summary>
/// Repository interface for account security data access.
/// </summary>
public interface IAccountSecurityRepository
{
    /// <summary>
    /// Get account security information by username.
    /// </summary>
    Task<AccountSecurity> GetByUsernameAsync(string username);
    
    /// <summary>
    /// Get account security information by account ID.
    /// </summary>
    Task<AccountSecurity> GetByAccountIdAsync(string accountId);
}
