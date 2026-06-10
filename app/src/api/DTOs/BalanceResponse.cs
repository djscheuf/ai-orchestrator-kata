namespace FinancialApp.API.DTOs;

public record BalanceResponse(
    string AccountId,
    string AccountName,
    decimal Balance
);
