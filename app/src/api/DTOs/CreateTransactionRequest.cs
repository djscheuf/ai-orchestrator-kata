namespace FinancialApp.API.DTOs;

public record CreateTransactionRequest(
    string TargetAccountId,
    decimal Amount,
    string Date
);
