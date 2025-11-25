using System;
using System.Collections.Generic;

namespace BenyFinance.Domain.Entities;

public class Category
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    
    public Guid UserId { get; set; }
    public User? User { get; set; }

    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public ICollection<RecurringTemplate> RecurringTemplates { get; set; } = new List<RecurringTemplate>();
}
