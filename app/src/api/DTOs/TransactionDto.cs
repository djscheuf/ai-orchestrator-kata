namespace FinancialApp.API.DTOs;

public record TransactionDto(
    string Id,
    string SourceAccountId,
    string TargetAccountId,
    decimal Amount,
    string Date,
    string? CreatedAt = null
);
