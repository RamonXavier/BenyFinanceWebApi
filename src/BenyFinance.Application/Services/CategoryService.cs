using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BenyFinance.Application.DTOs;
using BenyFinance.Application.Interfaces;
using BenyFinance.Domain.Entities;
using BenyFinance.Domain.Interfaces;

namespace BenyFinance.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<IEnumerable<CategoryDto>> GetAllAsync(Guid userId)
    {
        var categories = await _categoryRepository.GetAllByUserIdAsync(userId);
        return categories.Select(c => new CategoryDto(c.Id, c.Name, c.Color));
    }

    public async Task<CategoryDto?> GetByIdAsync(Guid id, Guid userId)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null || category.UserId != userId)
        {
            return null;
        }
        return new CategoryDto(category.Id, category.Name, category.Color);
    }

    public async Task<CategoryDto> CreateAsync(Guid userId, CreateCategoryDto createDto)
    {
        var category = new Category
        {
            Name = createDto.Name,
            Color = createDto.Color,
            UserId = userId
        };

        await _categoryRepository.AddAsync(category);
        return new CategoryDto(category.Id, category.Name, category.Color);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid userId)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null || category.UserId != userId)
        {
            return false;
        }

        await _categoryRepository.DeleteAsync(category);
        return true;
    }
}
