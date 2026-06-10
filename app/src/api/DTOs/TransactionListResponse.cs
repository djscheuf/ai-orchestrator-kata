using System.Collections.Generic;

namespace FinancialApp.API.DTOs;

public record TransactionListResponse(
    IEnumerable<TransactionDto> Transactions,
    decimal Balance
);
