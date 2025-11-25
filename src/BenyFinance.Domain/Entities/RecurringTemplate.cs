using System;

namespace BenyFinance.Domain.Entities;

public class RecurringTemplate
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    
    public Guid CategoryId { get; set; }
    public Category? Category { get; set; }

    public Guid UserId { get; set; }
    public User? User { get; set; }
}
