using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BenyFinance.Application.DTOs;

namespace BenyFinance.Application.Interfaces;

public interface ICreditCardService
{
    Task<IEnumerable<CreditCardDto>> GetAllAsync(Guid userId);
    Task<CreditCardDto?> GetByIdAsync(Guid id, Guid userId);
    Task<CreditCardDto> CreateAsync(Guid userId, CreateCreditCardDto createDto);
    Task<bool> DeleteAsync(Guid id, Guid userId);
}
