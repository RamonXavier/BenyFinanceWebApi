using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BenyFinance.Application.DTOs;
using BenyFinance.Application.Interfaces;
using BenyFinance.Domain.Entities;
using BenyFinance.Domain.Interfaces;

namespace BenyFinance.Application.Services;

public class RecurringTemplateService : IRecurringTemplateService
{
    private readonly IRecurringTemplateRepository _repository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICategoryRepository _categoryRepository;

    public RecurringTemplateService(
        IRecurringTemplateRepository repository,
        ITransactionRepository transactionRepository,
        ICategoryRepository categoryRepository)
    {
        _repository = repository;
        _transactionRepository = transactionRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<IEnumerable<RecurringTemplateDto>> GetAllAsync(Guid userId)
    {
        var templates = await _repository.GetAllByUserIdAsync(userId);
        return templates.Select(t => new RecurringTemplateDto(
            t.Id,
            t.Description,
            t.Amount,
            t.Category?.Name ?? "Unknown",
            t.CategoryId
        ));
    }

    public async Task<RecurringTemplateDto?> GetByIdAsync(Guid id, Guid userId)
    {
        var t = await _repository.GetByIdAsync(id);
        if (t == null || t.UserId != userId) return null;
        return new RecurringTemplateDto(t.Id, t.Description, t.Amount, t.Category?.Name ?? "Unknown", t.CategoryId);
    }

    public async Task<RecurringTemplateDto> CreateAsync(Guid userId, CreateRecurringTemplateDto createDto)
    {
        Guid categoryId;
        if (createDto.CategoryId.HasValue)
        {
            categoryId = createDto.CategoryId.Value;
        }
        else
        {
            // Find or create category by name
             var categories = await _categoryRepository.GetAllByUserIdAsync(userId);
            var category = categories.FirstOrDefault(c => c.Name.Equals(createDto.Category, StringComparison.OrdinalIgnoreCase));
            
            if (category == null)
            {
                 if (!string.IsNullOrEmpty(createDto.Category))
                {
                    category = new Category { Name = createDto.Category, Color = "#000000", UserId = userId };
                    await _categoryRepository.AddAsync(category);
                }
                else
                {
                    throw new Exception("Category is required");
                }
            }
            categoryId = category.Id;
        }

        var template = new RecurringTemplate
        {
            Description = createDto.Description,
            Amount = createDto.Amount,
            CategoryId = categoryId,
            UserId = userId
        };

        await _repository.AddAsync(template);
        
        var cat = await _categoryRepository.GetByIdAsync(categoryId);
        template.Category = cat;

        return new RecurringTemplateDto(template.Id, template.Description, template.Amount, template.Category?.Name ?? "Unknown", template.CategoryId);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid userId)
    {
        var t = await _repository.GetByIdAsync(id);
        if (t == null || t.UserId != userId) return false;
        await _repository.DeleteAsync(t);
        return true;
    }

    public async Task<IEnumerable<TransactionDto>> GenerateMonthlyTransactionsAsync(Guid userId, int month, int year)
    {
        var templates = await _repository.GetAllByUserIdAsync(userId);
        var transactions = new List<Transaction>();

        foreach (var template in templates)
        {
            var transaction = new Transaction
            {
                Date = new DateTime(year, month, 10), // Fixed day 10 as per requirements
                Description = template.Description,
                Amount = template.Amount, // Or 0.00 as per requirements option, but usually template amount. Let's use template amount.
                Type = TransactionType.Expense, // Usually recurring are bills/expenses.
                CategoryId = template.CategoryId,
                PaymentMethod = PaymentMethod.Cash, // Default
                Status = TransactionStatus.Pending,
                UserId = userId
            };
            transactions.Add(transaction);
        }

        await _transactionRepository.AddRangeAsync(transactions);

        // Return created transactions
        return transactions.Select(t => new TransactionDto(
            t.Id,
            t.Date,
            t.Description,
            t.Amount,
            t.Type.ToString().ToLower(),
            t.Category?.Name ?? "Unknown", // Category might not be loaded if we just added it, but template has it?
                                           // Actually template.Category is loaded in GetAllByUserIdAsync.
                                           // So we can assign it to transaction object before mapping if we want correct name in response.
            t.CategoryId,
            t.PaymentMethod.ToString().ToLower(),
            t.CardId,
            t.Status.ToString().ToLower()
        ));
    }
}
