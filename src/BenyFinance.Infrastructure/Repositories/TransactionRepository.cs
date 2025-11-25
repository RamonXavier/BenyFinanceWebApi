using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BenyFinance.Domain.Entities;
using BenyFinance.Domain.Interfaces;
using BenyFinance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BenyFinance.Infrastructure.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly AppDbContext _context;

    public TransactionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Transaction?> GetByIdAsync(Guid id)
    {
        return await _context.Transactions
            .Include(t => t.Category)
            .Include(t => t.Card)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<IEnumerable<Transaction>> GetAllByUserIdAsync(Guid userId, int? month, int? year, TransactionType? type)
    {
        var query = _context.Transactions
            .Include(t => t.Category)
            .Include(t => t.Card)
            .Where(t => t.UserId == userId);

        if (month.HasValue)
        {
            query = query.Where(t => t.Date.Month == month.Value);
        }

        if (year.HasValue)
        {
            query = query.Where(t => t.Date.Year == year.Value);
        }

        if (type.HasValue)
        {
            query = query.Where(t => t.Type == type.Value);
        }

        return await query.OrderByDescending(t => t.Date).ToListAsync();
    }

    public async Task<IEnumerable<Transaction>> GetRecentByUserIdAsync(Guid userId, int count)
    {
        return await _context.Transactions
            .Include(t => t.Category)
            .Include(t => t.Card)
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.Date)
            .Take(count)
            .ToListAsync();
    }

    public async Task AddAsync(Transaction transaction)
    {
        await _context.Transactions.AddAsync(transaction);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Transaction transaction)
    {
        _context.Transactions.Update(transaction);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Transaction transaction)
    {
        _context.Transactions.Remove(transaction);
        await _context.SaveChangesAsync();
    }

    public async Task AddRangeAsync(IEnumerable<Transaction> transactions)
    {
        await _context.Transactions.AddRangeAsync(transactions);
        await _context.SaveChangesAsync();
    }
}
