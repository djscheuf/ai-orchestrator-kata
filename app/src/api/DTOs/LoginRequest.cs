namespace FinancialApp.API.DTOs;

public record LoginRequest(
    string Username,
    string Password
);
