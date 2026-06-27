using Application.Interfaces.Repository;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class WeekRepository : IWeekRepository
{
    private readonly AppDbContext _context;

    public WeekRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Weeks?> GetByIdAsync(int id)
    {
        return await _context.Weeks
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Id == id);
    }

    public async Task<List<Weeks>> GetAllAsync()
    {
        return await _context.Weeks
            .AsNoTracking()
            .OrderBy(w => w.StartDate)
            .ToListAsync();
    }

    public async Task<bool> HasDateIntersectionAsync(DateTime startDate, DateTime endDate, int? excludedId = null)
    {
        IQueryable<Weeks> query = _context.Weeks.AsNoTracking();

        if (excludedId.HasValue)
        {
            query = query.Where(w => w.Id != excludedId.Value);
        }

        return await query.AnyAsync(w => startDate < w.EndDate && endDate > w.StartDate);
    }

    public async Task AddAsync(Weeks week)
    {
        await _context.Weeks.AddAsync(week);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Weeks week)
    {
        _context.Weeks.Update(week);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Weeks week)
    {
        _context.Weeks.Remove(week);
        await _context.SaveChangesAsync();
    }
}
