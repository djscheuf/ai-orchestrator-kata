using System;
using System.Threading.Tasks;

namespace FinancialApp.API.Services;

/// <summary>
/// Account information DTO for security service.
/// </summary>
public record AccountInfo(string AccountId, string AccountName);

/// <summary>
/// Service for account security operations (authentication, credential validation).
/// </summary>
public interface IAccountSecurityService
{
    /// <summary>
    /// Validate user credentials and return account ID if valid.
    /// </summary>
    Task<string> ValidateCredentialsAsync(string username, string password);
    
    /// <summary>
    /// Get account information by account ID.
    /// </summary>
    Task<AccountInfo> GetAccountInfoAsync(string accountId);
}

public class AccountSecurityService : IAccountSecurityService
{
    // Mock data for demonstration
    private readonly Dictionary<string, (string PasswordHash, string AccountId, string AccountName)> _users = new()
    {
        { "user_a", ("hash_a", "550e8400-e29b-41d4-a716-446655440001", "Alice Account") },
        { "user_b", ("hash_b", "550e8400-e29b-41d4-a716-446655440002", "Bob Account") },
        { "user_c", ("hash_c", "550e8400-e29b-41d4-a716-446655440003", "Charlie Account") },
        { "user_d", ("hash_d", "550e8400-e29b-41d4-a716-446655440004", "Diana Account") },
        { "user_e", ("hash_e", "550e8400-e29b-41d4-a716-446655440005", "Eve Account") }
    };
    
    public async Task<string> ValidateCredentialsAsync(string username, string password)
    {
        // Mock validation - in production, this would hash the password and compare
        if (_users.TryGetValue(username, out var user))
        {
            // For mock purposes, accept any non-empty password
            if (!string.IsNullOrEmpty(password))
            {
                return user.AccountId;
            }
        }
        
        return null;
    }
    
    public async Task<AccountInfo> GetAccountInfoAsync(string accountId)
    {
        var user = _users.Values.FirstOrDefault(u => u.AccountId == accountId);
        if (user != default)
        {
            return new AccountInfo(user.AccountId, user.AccountName);
        }
        
        return null;
    }
}
