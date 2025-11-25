using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BenyFinance.Application.DTOs;

namespace BenyFinance.Application.Interfaces;

public interface IRecurringTemplateService
{
    Task<IEnumerable<RecurringTemplateDto>> GetAllAsync(Guid userId);
    Task<RecurringTemplateDto?> GetByIdAsync(Guid id, Guid userId);
    Task<RecurringTemplateDto> CreateAsync(Guid userId, CreateRecurringTemplateDto createDto);
    Task<bool> DeleteAsync(Guid id, Guid userId);
    Task<IEnumerable<TransactionDto>> GenerateMonthlyTransactionsAsync(Guid userId, int month, int year);
}
