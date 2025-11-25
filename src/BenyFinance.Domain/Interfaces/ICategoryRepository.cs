using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BenyFinance.Domain.Entities;

namespace BenyFinance.Domain.Interfaces;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(Guid id);
    Task<IEnumerable<Category>> GetAllByUserIdAsync(Guid userId);
    Task AddAsync(Category category);
    Task DeleteAsync(Category category);
}
