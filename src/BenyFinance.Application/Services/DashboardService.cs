using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BenyFinance.Application.DTOs;
using BenyFinance.Application.Interfaces;
using BenyFinance.Domain.Entities;
using BenyFinance.Domain.Interfaces;

namespace BenyFinance.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly ITransactionRepository _transactionRepository;

    public DashboardService(ITransactionRepository transactionRepository)
    {
        _transactionRepository = transactionRepository;
    }

    public async Task<DashboardDto> GetDashboardDataAsync(Guid userId, int month, int year)
    {
        // 1. Get transactions for the month
        var transactions = await _transactionRepository.GetAllByUserIdAsync(userId, month, year, null);
        
        // 2. Calculate totals
        decimal income = transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
        decimal expense = transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
        
        // Balance = Income - Expense (Cash only? Or all? Usually all for balance, but requirement says "Saldo (Receitas - Despesas em Dinheiro)")
        // Let's assume Balance = Income - Expense for simplicity, or filter by PaymentMethod.Cash if strictly requested.
        // "balance": 1500.00, // Saldo (Receitas - Despesas em Dinheiro)
        // Let's try to follow requirement:
        decimal cashExpense = transactions.Where(t => t.Type == TransactionType.Expense && t.PaymentMethod == PaymentMethod.Cash).Sum(t => t.Amount);
        decimal balance = income - cashExpense; // Assuming all income is cash/bank, not credit.

        decimal cardExpenses = transactions.Where(t => t.Type == TransactionType.Expense && t.PaymentMethod == PaymentMethod.CreditCard).Sum(t => t.Amount);

        // 3. Recent transactions (last 5)
        var recentTransactions = await _transactionRepository.GetRecentByUserIdAsync(userId, 5);
        var recentDtos = recentTransactions.Select(t => new TransactionDto(
            t.Id, t.Date, t.Description, t.Amount, t.Type.ToString().ToLower(),
            t.Category?.Name ?? "Unknown", t.CategoryId, t.PaymentMethod.ToString().ToLower(), t.CardId, t.Status.ToString().ToLower()
        ));

        // 4. Bar Chart Data (Last 6 months)
        var barChartData = new List<BarChartDataDto>();
        for (int i = 5; i >= 0; i--)
        {
            var date = new DateTime(year, month, 1).AddMonths(-i);
            var monthTrans = await _transactionRepository.GetAllByUserIdAsync(userId, date.Month, date.Year, null);
            
            var monthIncome = monthTrans.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
            var monthExpense = monthTrans.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
            
            barChartData.Add(new BarChartDataDto(
                date.ToString("MMM"), // "Jan", "Feb", etc.
                monthIncome,
                monthExpense
            ));
        }

        // 5. Pie Chart Data (Expenses by Category)
        var pieChartData = transactions
            .Where(t => t.Type == TransactionType.Expense)
            .GroupBy(t => t.Category?.Name ?? "Unknown")
            .Select(g => new PieChartDataDto(g.Key, g.Sum(t => t.Amount)))
            .ToList();

        return new DashboardDto(
            balance,
            income,
            expense,
            cardExpenses,
            recentDtos,
            barChartData,
            pieChartData
        );
    }
}
