using System;
using BenyFinance.Domain.Entities;

namespace BenyFinance.Application.DTOs;

public record TransactionDto(
    Guid Id,
    DateTime Date,
    string Description,
    decimal Amount,
    string Type,
    string Category,
    Guid CategoryId,
    string PaymentMethod,
    Guid? CardId,
    string Status
);

public record CreateTransactionDto(
    DateTime Date,
    string Description,
    decimal Amount,
    string Type, // "income" or "expense"
    string Category, // Name of category (optional if CategoryId provided, but frontend sends name sometimes?)
                     // Actually frontend sends name in the example body: "category": "Salário"
                     // But we should support ID too or find by name.
    Guid? CategoryId,
    string PaymentMethod, // "cash" or "credit_card"
    Guid? CardId,
    string Status
);

public record UpdateTransactionDto(
    DateTime? Date,
    string? Description,
    decimal? Amount,
    string? Type,
    Guid? CategoryId,
    string? PaymentMethod,
    Guid? CardId,
    string? Status
);
