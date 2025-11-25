using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BenyFinance.Domain.Entities;

namespace BenyFinance.Domain.Interfaces;

public interface ICreditCardRepository
{
    Task<CreditCard?> GetByIdAsync(Guid id);
    Task<IEnumerable<CreditCard>> GetAllByUserIdAsync(Guid userId);
    Task AddAsync(CreditCard card);
    Task DeleteAsync(CreditCard card);
}
