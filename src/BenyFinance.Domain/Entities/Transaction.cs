using System;

namespace BenyFinance.Domain.Entities;

public enum TransactionType
{
    Income,
    Expense
}

public enum PaymentMethod
{
    Cash,
    CreditCard
}

public enum TransactionStatus
{
    Paid,
    Pending,
    Canceled
}

public class Transaction
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public TransactionType Type { get; set; }
    
    public Guid CategoryId { get; set; }
    public Category? Category { get; set; }

    public PaymentMethod PaymentMethod { get; set; }
    
    public Guid? CardId { get; set; }
    public CreditCard? Card { get; set; }

    public TransactionStatus Status { get; set; }

    public Guid UserId { get; set; }
    public User? User { get; set; }
}
