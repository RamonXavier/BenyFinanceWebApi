using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BenyFinance.Application.DTOs;

namespace BenyFinance.Application.Interfaces;

public interface ITransactionService
{
    Task<IEnumerable<TransactionDto>> GetAllAsync(Guid userId, int? month, int? year, string? type);
    Task<TransactionDto?> GetByIdAsync(Guid id, Guid userId);
    Task<TransactionDto> CreateAsync(Guid userId, CreateTransactionDto createDto);
    Task<TransactionDto?> UpdateAsync(Guid id, Guid userId, UpdateTransactionDto updateDto);
    Task<bool> DeleteAsync(Guid id, Guid userId);
}
