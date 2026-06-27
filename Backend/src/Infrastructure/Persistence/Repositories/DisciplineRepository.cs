using Application.Interfaces.Repository;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class DisciplineRepository : IDisciplineRepository
{
    private readonly AppDbContext _context;

    public DisciplineRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Discipline?> GetByIdAsync(int id)
    {
        return await _context.Disciplines
            .AsNoTracking()
            .Include(d => d.Subject)
            .Include(d => d.Teacher)
            .Include(d => d.Group)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<List<Discipline>> GetAllAsync()
    {
        return await _context.Disciplines
            .AsNoTracking()
            .Include(d => d.Subject)
            .Include(d => d.Teacher)
            .Include(d => d.Group)
            .OrderBy(d => d.Subject.Name)
            .ThenBy(d => d.Group.Name)
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(int subjectId, int teacherId, int groupId, int? excludedId = null)
    {
        IQueryable<Discipline> query = _context.Disciplines
            .AsNoTracking()
            .Where(d => d.SubjectId == subjectId && d.TeacherId == teacherId && d.GroupId == groupId);

        if (excludedId.HasValue)
        {
            query = query.Where(d => d.Id != excludedId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task AddAsync(Discipline discipline)
    {
        await _context.Disciplines.AddAsync(discipline);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Discipline discipline)
    {
        _context.Disciplines.Update(discipline);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Discipline discipline)
    {
        _context.Disciplines.Remove(discipline);
        await _context.SaveChangesAsync();
    }
}
