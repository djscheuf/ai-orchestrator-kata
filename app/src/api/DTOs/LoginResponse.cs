namespace FinancialApp.API.DTOs;

public record LoginResponse(
    string Token,
    string AccountId,
    string AccountName
);
