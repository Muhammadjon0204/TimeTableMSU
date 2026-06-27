using Application.Interfaces.Repository;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class SubjectRepository : ISubjectRepository
{
    private readonly AppDbContext _context;

    public SubjectRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Subject?> GetByIdAsync(int id)
    {
        return await _context.Subjects
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<List<Subject>> GetAllAsync()
    {
        return await _context.Subjects
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<bool> ExistsByNameAndSemesterAsync(string name, short semester, int? excludedId = null)
    {
        string normalizedName = name.Trim().ToLower();

        IQueryable<Subject> query = _context.Subjects
            .AsNoTracking()
            .Where(s => s.Semester == semester);

        if (excludedId.HasValue)
        {
            query = query.Where(s => s.Id != excludedId.Value);
        }

        return await query.AnyAsync(s => s.Name.ToLower() == normalizedName);
    }

    public async Task AddAsync(Subject subject)
    {
        await _context.Subjects.AddAsync(subject);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Subject subject)
    {
        _context.Subjects.Update(subject);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Subject subject)
    {
        _context.Subjects.Remove(subject);
        await _context.SaveChangesAsync();
    }
}
