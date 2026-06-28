using Application.Interfaces.Repository;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class AdminScheduleBoardRepository : IAdminScheduleBoardRepository
{
    private readonly AppDbContext _context;

    public AdminScheduleBoardRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Weeks>> GetWeeksAsync()
    {
        return await _context.Weeks
            .AsNoTracking()
            .OrderBy(week => week.StartDate)
            .ToListAsync();
    }

    public async Task<List<Group>> GetGroupsAsync()
    {
        return await _context.Groups
            .AsNoTracking()
            .OrderBy(group => group.Name)
            .ToListAsync();
    }

    public async Task<List<Teacher>> GetTeachersAsync()
    {
        return await _context.Teachers
            .AsNoTracking()
            .OrderBy(teacher => teacher.LastName)
            .ThenBy(teacher => teacher.FirstName)
            .ToListAsync();
    }

    public async Task<List<Audience>> GetAudiencesAsync()
    {
        return await _context.Audiences
            .AsNoTracking()
            .OrderBy(audience => audience.Number)
            .ToListAsync();
    }

    public async Task<List<Schedule>> GetWeeklySchedulesAsync(int weekId, int? groupId, int? teacherId, int? audienceId)
    {
        IQueryable<Schedule> query = _context.Schedules
            .AsNoTracking()
            .Include(schedule => schedule.Discipline)
            .ThenInclude(discipline => discipline.Subject)
            .Include(schedule => schedule.Teacher)
            .Include(schedule => schedule.Group)
            .Include(schedule => schedule.Audience)
            .Where(schedule => schedule.WeekId == weekId);

        if (groupId.HasValue)
        {
            query = query.Where(schedule => schedule.GroupId == groupId.Value);
        }

        if (teacherId.HasValue)
        {
            query = query.Where(schedule => schedule.TeacherId == teacherId.Value);
        }

        if (audienceId.HasValue)
        {
            query = query.Where(schedule => schedule.AudienceId == audienceId.Value);
        }

        return await query
            .OrderBy(schedule => schedule.Den)
            .ThenBy(schedule => schedule.Para)
            .ThenBy(schedule => schedule.Group.Name)
            .ToListAsync();
    }
}
