using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BenyFinance.Application.DTOs;
using BenyFinance.Application.Interfaces;
using BenyFinance.Domain.Entities;
using BenyFinance.Domain.Interfaces;

namespace BenyFinance.Application.Services;

public class TransactionService : ITransactionService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICreditCardRepository _creditCardRepository;

    public TransactionService(
        ITransactionRepository transactionRepository,
        ICategoryRepository categoryRepository,
        ICreditCardRepository creditCardRepository)
    {
        _transactionRepository = transactionRepository;
        _categoryRepository = categoryRepository;
        _creditCardRepository = creditCardRepository;
    }

    public async Task<IEnumerable<TransactionDto>> GetAllAsync(Guid userId, int? month, int? year, string? type)
    {
        TransactionType? transactionType = null;
        if (!string.IsNullOrEmpty(type) && Enum.TryParse<TransactionType>(type, true, out var parsedType))
        {
            transactionType = parsedType;
        }

        var transactions = await _transactionRepository.GetAllByUserIdAsync(userId, month, year, transactionType);
        return transactions.Select(MapToDto);
    }

    public async Task<TransactionDto?> GetByIdAsync(Guid id, Guid userId)
    {
        var transaction = await _transactionRepository.GetByIdAsync(id);
        if (transaction == null || transaction.UserId != userId)
        {
            return null;
        }
        return MapToDto(transaction);
    }

    public async Task<TransactionDto> CreateAsync(Guid userId, CreateTransactionDto createDto)
    {
        // Resolve Category
        Guid categoryId;
        if (createDto.CategoryId.HasValue)
        {
            categoryId = createDto.CategoryId.Value;
        }
        else
        {
            // Try to find by name or create default? 
            // For now, assume if no ID, we might fail or need logic.
            // Let's assume frontend sends ID if possible, or we search by name.
            // Simplified: Require ID or fail if not found by name.
            var categories = await _categoryRepository.GetAllByUserIdAsync(userId);
            var category = categories.FirstOrDefault(c => c.Name.Equals(createDto.Category, StringComparison.OrdinalIgnoreCase));
            
            if (category == null)
            {
                // Create new category if not exists? Or throw?
                // Let's create for better UX if name provided
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

        if (!Enum.TryParse<TransactionType>(createDto.Type, true, out var type))
        {
            throw new Exception("Invalid transaction type");
        }

        if (!Enum.TryParse<PaymentMethod>(createDto.PaymentMethod, true, out var paymentMethod))
        {
            paymentMethod = PaymentMethod.Cash; // Default
        }

        if (!Enum.TryParse<TransactionStatus>(createDto.Status, true, out var status))
        {
            status = TransactionStatus.Paid; // Default
        }

        var transaction = new Transaction
        {
            Date = createDto.Date,
            Description = createDto.Description,
            Amount = createDto.Amount,
            Type = type,
            CategoryId = categoryId,
            PaymentMethod = paymentMethod,
            CardId = createDto.CardId,
            Status = status,
            UserId = userId
        };

        await _transactionRepository.AddAsync(transaction);
        
        // Reload to get included entities if needed, or just map manually
        // For simplicity, we return what we created, but we might miss Category Name if we just used ID.
        // Let's fetch the category name if we have it.
        var cat = await _categoryRepository.GetByIdAsync(categoryId);
        transaction.Category = cat;

        return MapToDto(transaction);
    }

    public async Task<TransactionDto?> UpdateAsync(Guid id, Guid userId, UpdateTransactionDto updateDto)
    {
        var transaction = await _transactionRepository.GetByIdAsync(id);
        if (transaction == null || transaction.UserId != userId)
        {
            return null;
        }

        if (updateDto.Date.HasValue) transaction.Date = updateDto.Date.Value;
        if (updateDto.Description != null) transaction.Description = updateDto.Description;
        if (updateDto.Amount.HasValue) transaction.Amount = updateDto.Amount.Value;
        
        if (updateDto.Type != null && Enum.TryParse<TransactionType>(updateDto.Type, true, out var type))
        {
            transaction.Type = type;
        }

        if (updateDto.CategoryId.HasValue)
        {
            transaction.CategoryId = updateDto.CategoryId.Value;
        }

        if (updateDto.PaymentMethod != null && Enum.TryParse<PaymentMethod>(updateDto.PaymentMethod, true, out var pm))
        {
            transaction.PaymentMethod = pm;
        }

        if (updateDto.CardId.HasValue)
        {
            transaction.CardId = updateDto.CardId; // Can be null? updateDto.CardId is nullable Guid?
            // If we want to set to null, we need a way to express that. 
            // For now, assume if passed, update it. If null, ignore? Or if explicit null?
            // Standard DTO patch usually implies null = no change.
        }
        
        if (updateDto.Status != null && Enum.TryParse<TransactionStatus>(updateDto.Status, true, out var status))
        {
            transaction.Status = status;
        }

        await _transactionRepository.UpdateAsync(transaction);
        return MapToDto(transaction);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid userId)
    {
        var transaction = await _transactionRepository.GetByIdAsync(id);
        if (transaction == null || transaction.UserId != userId)
        {
            return false;
        }

        await _transactionRepository.DeleteAsync(transaction);
        return true;
    }

    private TransactionDto MapToDto(Transaction t)
    {
        return new TransactionDto(
            t.Id,
            t.Date,
            t.Description,
            t.Amount,
            t.Type.ToString().ToLower(),
            t.Category?.Name ?? "Unknown",
            t.CategoryId,
            t.PaymentMethod.ToString().ToLower(),
            t.CardId,
            t.Status.ToString().ToLower()
        );
    }
}
