using System;
using System.Threading.Tasks;
using BenyFinance.Domain.Entities;

namespace BenyFinance.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByEmailAsync(string email);
    Task AddAsync(User user);
    Task UpdateAsync(User user);
}
