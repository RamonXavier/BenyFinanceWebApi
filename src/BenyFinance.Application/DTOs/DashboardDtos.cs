using System.Collections.Generic;

namespace BenyFinance.Application.DTOs;

public record DashboardDto(
    decimal Balance,
    decimal Income,
    decimal Expense,
    decimal CardExpenses,
    IEnumerable<TransactionDto> Transactions,
    IEnumerable<BarChartDataDto> BarChartData,
    IEnumerable<PieChartDataDto> PieChartData
);

public record BarChartDataDto(string Name, decimal Receitas, decimal Despesas);

public record PieChartDataDto(string Name, decimal Value);
