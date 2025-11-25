using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BenyFinance.Application.DTOs;

namespace BenyFinance.Application.Interfaces;

public interface ICategoryService
{
    Task<IEnumerable<CategoryDto>> GetAllAsync(Guid userId);
    Task<CategoryDto?> GetByIdAsync(Guid id, Guid userId);
    Task<CategoryDto> CreateAsync(Guid userId, CreateCategoryDto createDto);
    Task<bool> DeleteAsync(Guid id, Guid userId);
}
