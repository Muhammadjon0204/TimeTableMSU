using Application.Interfaces.Repository;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class ExecutionRepository : IExecutionRepository
{
    private readonly AppDbContext _context;

    public ExecutionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Execution?> GetByIdAsync(int id)
    {
        return await _context.Executions
            .AsNoTracking()
            .Include(e => e.Schedule)
            .ThenInclude(s => s.Discipline)
            .ThenInclude(d => d.Subject)
            .Include(e => e.Schedule)
            .ThenInclude(s => s.Group)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<List<Execution>> GetAllAsync()
    {
        return await _context.Executions
            .AsNoTracking()
            .Include(e => e.Schedule)
            .ThenInclude(s => s.Discipline)
            .ThenInclude(d => d.Subject)
            .Include(e => e.Schedule)
            .ThenInclude(s => s.Group)
            .OrderByDescending(e => e.ExecutionDate)
            .ToListAsync();
    }

    public async Task AddAsync(Execution execution)
    {
        await _context.Executions.AddAsync(execution);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Execution execution)
    {
        _context.Executions.Update(execution);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Execution execution)
    {
        _context.Executions.Remove(execution);
        await _context.SaveChangesAsync();
    }
}
