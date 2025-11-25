using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BenyFinance.Domain.Entities;

namespace BenyFinance.Domain.Interfaces;

public interface IRecurringTemplateRepository
{
    Task<RecurringTemplate?> GetByIdAsync(Guid id);
    Task<IEnumerable<RecurringTemplate>> GetAllByUserIdAsync(Guid userId);
    Task AddAsync(RecurringTemplate template);
    Task DeleteAsync(RecurringTemplate template);
}
