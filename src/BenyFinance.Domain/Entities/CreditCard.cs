using System;
using System.Collections.Generic;

namespace BenyFinance.Domain.Entities;

public class CreditCard
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public decimal Limit { get; set; }
    public string Bank { get; set; } = string.Empty;
    public int ClosingDay { get; set; }
    public int DueDay { get; set; }

    public Guid UserId { get; set; }
    public User? User { get; set; }

    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
