using Application.Interfaces.Repository;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class SpecialityRepository : ISpecialityRepository
{
    private readonly AppDbContext _context;

    public SpecialityRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Speciality?> GetByIdAsync(int id)
    {
        return await _context.Specialities
            .AsNoTracking()
            .Include(s => s.Faculty)
            .Include(s => s.Groups)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<List<Speciality>> GetAllAsync()
    {
        return await _context.Specialities
            .AsNoTracking()
            .Include(s => s.Faculty)
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<bool> ExistsByNameAsync(string name, int facultyId, int? excludedId = null)
    {
        string normalizedName = name.Trim().ToLower();

        IQueryable<Speciality> query = _context.Specialities
            .AsNoTracking()
            .Where(s => s.FacultyId == facultyId);

        if (excludedId.HasValue)
        {
            query = query.Where(s => s.Id != excludedId.Value);
        }

        return await query.AnyAsync(s => s.Name.ToLower() == normalizedName);
    }

    public async Task AddAsync(Speciality speciality)
    {
        await _context.Specialities.AddAsync(speciality);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Speciality speciality)
    {
        _context.Specialities.Update(speciality);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Speciality speciality)
    {
        _context.Specialities.Remove(speciality);
        await _context.SaveChangesAsync();
    }
}
