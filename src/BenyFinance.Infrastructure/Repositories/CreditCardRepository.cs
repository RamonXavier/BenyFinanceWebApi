using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BenyFinance.Domain.Entities;
using BenyFinance.Domain.Interfaces;
using BenyFinance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BenyFinance.Infrastructure.Repositories;

public class CreditCardRepository : ICreditCardRepository
{
    private readonly AppDbContext _context;

    public CreditCardRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<CreditCard?> GetByIdAsync(Guid id)
    {
        return await _context.CreditCards.FindAsync(id);
    }

    public async Task<IEnumerable<CreditCard>> GetAllByUserIdAsync(Guid userId)
    {
        return await _context.CreditCards
            .Where(c => c.UserId == userId)
            .ToListAsync();
    }

    public async Task AddAsync(CreditCard card)
    {
        await _context.CreditCards.AddAsync(card);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(CreditCard card)
    {
        _context.CreditCards.Remove(card);
        await _context.SaveChangesAsync();
    }
}
