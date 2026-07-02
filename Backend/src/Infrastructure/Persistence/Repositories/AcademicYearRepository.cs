using Application.Interfaces.Repository;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class AcademicYearRepository : IAcademicYearRepository
{
    private readonly AppDbContext _context;

    public AcademicYearRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AcademicYear?> GetByIdAsync(int id)
    {
        return await _context.AcademicYears
            .Include(x => x.Weeks)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<AcademicYear?> GetCurrentAsync()
    {
        return await _context.AcademicYears
            .AsNoTracking()
            .Include(x => x.Weeks)
            .OrderByDescending(x => x.IsCurrent)
            .ThenByDescending(x => x.StartDate)
            .FirstOrDefaultAsync();
    }

    public async Task<List<AcademicYear>> GetAllAsync()
    {
        return await _context.AcademicYears
            .AsNoTracking()
            .Include(x => x.Weeks)
            .OrderByDescending(x => x.IsCurrent)
            .ThenBy(x => x.StartDate)
            .ToListAsync();
    }

    public async Task<bool> NameExistsAsync(string name, int? excludedId = null)
    {
        IQueryable<AcademicYear> query = _context.AcademicYears.AsNoTracking();

        if (excludedId.HasValue)
        {
            query = query.Where(x => x.Id != excludedId.Value);
        }

        return await query.AnyAsync(x => x.Name == name);
    }

    public async Task AddAsync(AcademicYear academicYear)
    {
        await _context.AcademicYears.AddAsync(academicYear);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(AcademicYear academicYear)
    {
        _context.AcademicYears.Update(academicYear);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(AcademicYear academicYear)
    {
        _context.AcademicYears.Remove(academicYear);
        await _context.SaveChangesAsync();
    }
}
