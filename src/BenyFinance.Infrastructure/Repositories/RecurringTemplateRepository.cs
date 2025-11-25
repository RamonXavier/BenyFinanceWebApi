using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BenyFinance.Domain.Entities;
using BenyFinance.Domain.Interfaces;
using BenyFinance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BenyFinance.Infrastructure.Repositories;

public class RecurringTemplateRepository : IRecurringTemplateRepository
{
    private readonly AppDbContext _context;

    public RecurringTemplateRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<RecurringTemplate?> GetByIdAsync(Guid id)
    {
        return await _context.RecurringTemplates
            .Include(r => r.Category)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<IEnumerable<RecurringTemplate>> GetAllByUserIdAsync(Guid userId)
    {
        return await _context.RecurringTemplates
            .Include(r => r.Category)
            .Where(r => r.UserId == userId)
            .ToListAsync();
    }

    public async Task AddAsync(RecurringTemplate template)
    {
        await _context.RecurringTemplates.AddAsync(template);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(RecurringTemplate template)
    {
        _context.RecurringTemplates.Remove(template);
        await _context.SaveChangesAsync();
    }
}
