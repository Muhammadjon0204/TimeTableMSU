using Application.Interfaces.Repository;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class AdminDashboardRepository : IAdminDashboardRepository
{
    private readonly AppDbContext _context;

    public AdminDashboardRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Weeks?> GetWeekAsync(int? weekId)
    {
        if (weekId.HasValue)
        {
            return await _context.Weeks
                .AsNoTracking()
                .Include(week => week.AcademicYear)
                .Include(week => week.AcademicPeriod)
                .FirstOrDefaultAsync(week => week.Id == weekId.Value);
        }

        DateTime today = DateTime.UtcNow.Date;

        Weeks? currentWeekWithSchedules = await _context.Weeks
            .AsNoTracking()
            .Include(week => week.AcademicYear)
            .Include(week => week.AcademicPeriod)
            .Where(week => week.StartDate.Date <= today && week.EndDate.Date >= today)
            .OrderByDescending(week => week.StartDate)
            .FirstOrDefaultAsync();

        if (currentWeekWithSchedules != null)
        {
            return currentWeekWithSchedules;
        }

        Weeks? nextWeek = await _context.Weeks
            .AsNoTracking()
            .Include(week => week.AcademicYear)
            .Include(week => week.AcademicPeriod)
            .Where(week => week.StartDate.Date > today)
            .OrderBy(week => week.StartDate)
            .FirstOrDefaultAsync();

        if (nextWeek != null)
        {
            return nextWeek;
        }

        return await _context.Weeks
            .AsNoTracking()
            .Include(week => week.AcademicYear)
            .Include(week => week.AcademicPeriod)
            .OrderByDescending(week => week.StartDate)
            .FirstOrDefaultAsync();
    }

    public async Task<List<Group>> GetGroupsAsync(int? groupId)
    {
        IQueryable<Group> query = _context.Groups.AsNoTracking();

        if (groupId.HasValue)
        {
            query = query.Where(group => group.Id == groupId.Value);
        }

        return await query
            .OrderBy(group => group.Name)
            .ToListAsync();
    }

    public async Task<Dictionary<int, int>> GetStudentCountsByGroupAsync(List<int> groupIds)
    {
        return await _context.Students
            .AsNoTracking()
            .Where(student => student.GroupId.HasValue && groupIds.Contains(student.GroupId.Value))
            .GroupBy(student => student.GroupId!.Value)
            .Select(group => new
            {
                GroupId = group.Key,
                Count = group.Count()
            })
            .ToDictionaryAsync(item => item.GroupId, item => item.Count);
    }

    public async Task<List<Schedule>> GetSchedulesAsync(int weekId, List<int> groupIds)
    {
        return await _context.Schedules
            .AsNoTracking()
            .Where(schedule => schedule.WeekId == weekId && groupIds.Contains(schedule.GroupId))
            .OrderBy(schedule => schedule.Den)
            .ThenBy(schedule => schedule.Para)
            .ToListAsync();
    }

    public async Task<List<Attendance>> GetAttendancesAsync(int weekId, List<int> groupIds)
    {
        return await _context.Attendances
            .AsNoTracking()
            .Include(attendance => attendance.Student)
            .Where(attendance =>
                attendance.WeekId == weekId &&
                attendance.Student.GroupId.HasValue &&
                groupIds.Contains(attendance.Student.GroupId.Value))
            .OrderBy(attendance => attendance.Day)
            .ThenBy(attendance => attendance.Para)
            .ToListAsync();
    }
}
