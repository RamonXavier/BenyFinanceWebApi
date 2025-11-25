using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BenyFinance.Domain.Entities;

namespace BenyFinance.Domain.Interfaces;

public interface ITransactionRepository
{
    Task<Transaction?> GetByIdAsync(Guid id);
    Task<IEnumerable<Transaction>> GetAllByUserIdAsync(Guid userId, int? month, int? year, TransactionType? type);
    Task<IEnumerable<Transaction>> GetRecentByUserIdAsync(Guid userId, int count);
    Task AddAsync(Transaction transaction);
    Task UpdateAsync(Transaction transaction);
    Task DeleteAsync(Transaction transaction);
    Task AddRangeAsync(IEnumerable<Transaction> transactions);
}
