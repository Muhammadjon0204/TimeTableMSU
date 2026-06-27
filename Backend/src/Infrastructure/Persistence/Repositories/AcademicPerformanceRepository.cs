using Application.Common;
using Application.Interfaces.Repository;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class AcademicPerformanceRepository : IAcademicPerformanceRepository
{
    private readonly AppDbContext _context;

    public AcademicPerformanceRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AcademicPerformance?> GetByIdAsync(int id)
    {
        return await _context.AcademicPerformances
            .AsNoTracking()
            .Include(ap => ap.Student)
            .Include(ap => ap.Discipline)
            .ThenInclude(d => d.Subject)
            .Include(ap => ap.Teacher)
            .FirstOrDefaultAsync(ap => ap.Id == id);
    }

    public async Task<List<AcademicPerformance>> GetAllAsync()
    {
        return await _context.AcademicPerformances
            .AsNoTracking()
            .Include(ap => ap.Student)
            .Include(ap => ap.Discipline)
            .ThenInclude(d => d.Subject)
            .Include(ap => ap.Teacher)
            .OrderBy(ap => ap.Student.LastName)
            .ThenBy(ap => ap.Student.FirstName)
            .ToListAsync();
    }

    public async Task<PagedResult<AcademicPerformance>> GetPagedAsync(int pageNumber, int pageSize)
    {
        IQueryable<AcademicPerformance> query = _context.AcademicPerformances
            .AsNoTracking()
            .Include(ap => ap.Student)
            .Include(ap => ap.Discipline)
            .ThenInclude(d => d.Subject)
            .Include(ap => ap.Teacher);

        int totalCount = await query.CountAsync();

        List<AcademicPerformance> academicPerformances = await query
            .OrderBy(ap => ap.Student.LastName)
            .ThenBy(ap => ap.Student.FirstName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<AcademicPerformance>(academicPerformances, totalCount, pageNumber, pageSize);
    }

    public async Task<bool> ExistsAsync(int studentId, int disciplineId, int teacherId, int? excludedId = null)
    {
        IQueryable<AcademicPerformance> query = _context.AcademicPerformances
            .AsNoTracking()
            .Where(ap => ap.StudentId == studentId && ap.DisciplineId == disciplineId && ap.TeacherId == teacherId);

        if (excludedId.HasValue)
        {
            query = query.Where(ap => ap.Id != excludedId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task AddAsync(AcademicPerformance academicPerformance)
    {
        await _context.AcademicPerformances.AddAsync(academicPerformance);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(AcademicPerformance academicPerformance)
    {
        _context.AcademicPerformances.Update(academicPerformance);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(AcademicPerformance academicPerformance)
    {
        _context.AcademicPerformances.Remove(academicPerformance);
        await _context.SaveChangesAsync();
    }
}
