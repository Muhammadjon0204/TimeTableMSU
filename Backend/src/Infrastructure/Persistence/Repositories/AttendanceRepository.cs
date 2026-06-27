using Application.Common;
using Application.Interfaces.Repository;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class AttendanceRepository : IAttendanceRepository
{
    private readonly AppDbContext _context;

    public AttendanceRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Attendance?> GetByIdAsync(int id)
    {
        return await _context.Attendances
            .AsNoTracking()
            .Include(a => a.Student)
            .Include(a => a.Week)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<List<Attendance>> GetAllAsync()
    {
        return await _context.Attendances
            .AsNoTracking()
            .Include(a => a.Student)
            .Include(a => a.Week)
            .OrderBy(a => a.Week.StartDate)
            .ThenBy(a => a.Day)
            .ThenBy(a => a.Para)
            .ToListAsync();
    }

    public async Task<PagedResult<Attendance>> GetPagedAsync(int pageNumber, int pageSize)
    {
        IQueryable<Attendance> query = _context.Attendances
            .AsNoTracking()
            .Include(a => a.Student)
            .Include(a => a.Week);

        int totalCount = await query.CountAsync();

        List<Attendance> attendances = await query
            .OrderBy(a => a.Week.StartDate)
            .ThenBy(a => a.Day)
            .ThenBy(a => a.Para)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<Attendance>(attendances, totalCount, pageNumber, pageSize);
    }

    public async Task<bool> ExistsDuplicateAsync(int studentId, int weekId, short day, short para, int? excludedId = null)
    {
        IQueryable<Attendance> query = _context.Attendances
            .AsNoTracking()
            .Where(a => a.StudentId == studentId && a.WeekId == weekId && a.Day == day && a.Para == para);

        if (excludedId.HasValue)
        {
            query = query.Where(a => a.Id != excludedId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task AddAsync(Attendance attendance)
    {
        await _context.Attendances.AddAsync(attendance);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Attendance attendance)
    {
        _context.Attendances.Update(attendance);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Attendance attendance)
    {
        _context.Attendances.Remove(attendance);
        await _context.SaveChangesAsync();
    }
}
