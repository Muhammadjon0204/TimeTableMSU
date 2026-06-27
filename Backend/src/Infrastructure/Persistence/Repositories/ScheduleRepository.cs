using Application.Common;
using Application.Interfaces.Repository;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class ScheduleRepository : IScheduleRepository
{
    private readonly AppDbContext _context;

    public ScheduleRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Schedule?> GetByIdAsync(int id)
    {
        return await QueryWithDetails()
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<List<Schedule>> GetAllAsync()
    {
        return await QueryWithDetails()
            .OrderBy(s => s.Week.StartDate)
            .ThenBy(s => s.Den)
            .ThenBy(s => s.Para)
            .ToListAsync();
    }

    public async Task<PagedResult<Schedule>> GetPagedAsync(int pageNumber, int pageSize)
    {
        IQueryable<Schedule> query = QueryWithDetails();

        int totalCount = await query.CountAsync();

        List<Schedule> schedules = await query
            .OrderBy(s => s.Week.StartDate)
            .ThenBy(s => s.Den)
            .ThenBy(s => s.Para)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<Schedule>(schedules, totalCount, pageNumber, pageSize);
    }

    public async Task<List<Schedule>> GetByGroupAndWeekAsync(int groupId, int weekId)
    {
        return await QueryWithDetails()
            .Where(s => s.GroupId == groupId && s.WeekId == weekId)
            .OrderBy(s => s.Den)
            .ThenBy(s => s.Para)
            .ToListAsync();
    }

    public async Task<List<Schedule>> GetByTeacherAndWeekAsync(int teacherId, int weekId)
    {
        return await QueryWithDetails()
            .Where(s => s.TeacherId == teacherId && s.WeekId == weekId)
            .OrderBy(s => s.Den)
            .ThenBy(s => s.Para)
            .ToListAsync();
    }

    public async Task<bool> HasCollisionAsync(short den, short para, int weekId, int? audienceId, int? groupId, int? teacherId, int? excludedId = null)
    {
        IQueryable<Schedule> query = _context.Schedules
            .AsNoTracking()
            .Where(s => s.Den == den && s.Para == para && s.WeekId == weekId);

        if (excludedId.HasValue)
        {
            query = query.Where(s => s.Id != excludedId.Value);
        }

        return await query.AnyAsync(s =>
            (audienceId.HasValue && s.AudienceId == audienceId.Value) ||
            (groupId.HasValue && s.GroupId == groupId.Value) ||
            (teacherId.HasValue && s.TeacherId == teacherId.Value));
    }

    public async Task AddAsync(Schedule schedule)
    {
        await _context.Schedules.AddAsync(schedule);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Schedule schedule)
    {
        _context.Schedules.Update(schedule);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Schedule schedule)
    {
        _context.Schedules.Remove(schedule);
        await _context.SaveChangesAsync();
    }

    private IQueryable<Schedule> QueryWithDetails()
    {
        return _context.Schedules
            .AsNoTracking()
            .Include(s => s.Discipline)
            .ThenInclude(d => d.Subject)
            .Include(s => s.Teacher)
            .Include(s => s.Audience)
            .Include(s => s.Group)
            .Include(s => s.Week);
    }
}
