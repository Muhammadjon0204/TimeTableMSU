using Application.Interfaces.Repository;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class FacultyRepository : IFacultyRepository
{
    private readonly AppDbContext _context;

    public FacultyRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Faculty?> GetByIdAsync(int id)
    {
        return await _context.Faculties
            .AsNoTracking()
            .Include(f => f.Specialities)
            .FirstOrDefaultAsync(f => f.Id == id);
    }

    public async Task<List<Faculty>> GetAllAsync()
    {
        return await _context.Faculties
            .AsNoTracking()
            .OrderBy(f => f.Name)
            .ToListAsync();
    }

    public async Task<bool> ExistsByNameAsync(string name, int? excludedId = null)
    {
        string normalizedName = name.Trim().ToLower();

        IQueryable<Faculty> query = _context.Faculties.AsNoTracking();

        if (excludedId.HasValue)
        {
            query = query.Where(f => f.Id != excludedId.Value);
        }

        return await query.AnyAsync(f => f.Name.ToLower() == normalizedName);
    }

    public async Task AddAsync(Faculty faculty)
    {
        await _context.Faculties.AddAsync(faculty);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Faculty faculty)
    {
        _context.Faculties.Update(faculty);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Faculty faculty)
    {
        _context.Faculties.Remove(faculty);
        await _context.SaveChangesAsync();
    }
}
