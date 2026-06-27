using Application.Interfaces.Repository;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class AudienceRepository : IAudienceRepository
{
    private readonly AppDbContext _context;

    public AudienceRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Audience?> GetByIdAsync(int id)
    {
        return await _context.Audiences
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<List<Audience>> GetAllAsync()
    {
        return await _context.Audiences
            .AsNoTracking()
            .OrderBy(a => a.Number)
            .ToListAsync();
    }

    public async Task<bool> ExistsByNumberAsync(string number, int? excludedId = null)
    {
        string normalizedNumber = number.Trim().ToLower();

        IQueryable<Audience> query = _context.Audiences.AsNoTracking();

        if (excludedId.HasValue)
        {
            query = query.Where(a => a.Id != excludedId.Value);
        }

        return await query.AnyAsync(a => a.Number.ToLower() == normalizedNumber);
    }

    public async Task AddAsync(Audience audience)
    {
        await _context.Audiences.AddAsync(audience);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Audience audience)
    {
        _context.Audiences.Update(audience);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Audience audience)
    {
        _context.Audiences.Remove(audience);
        await _context.SaveChangesAsync();
    }
}
