using Application.Interfaces.Repository;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class PortalRepository : IPortalRepository
{
    private readonly AppDbContext _context;

    public PortalRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Student?> GetStudentByEmailAsync(string email)
    {
        return await _context.Students
            .AsNoTracking()
            .Include(s => s.Group)
            .FirstOrDefaultAsync(s => s.Email != null && s.Email.ToLower() == email);
    }

    public async Task<Teacher?> GetTeacherByEmailAsync(string email)
    {
        return await _context.Teachers
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Email != null && t.Email.ToLower() == email);
    }

    public async Task<List<Schedule>> GetSchedulesByGroupAsync(int groupId)
    {
        return await ScheduleQuery()
            .Where(s => s.GroupId == groupId)
            .OrderBy(s => s.Week.StartDate)
            .ThenBy(s => s.Den)
            .ThenBy(s => s.Para)
            .ToListAsync();
    }

    public async Task<List<AcademicPerformance>> GetAcademicPerformancesByStudentAsync(int studentId)
    {
        return await AcademicPerformanceQuery()
            .Where(ap => ap.StudentId == studentId)
            .OrderBy(ap => ap.Discipline.Subject.Name)
            .ToListAsync();
    }

    public async Task<List<Attendance>> GetAttendancesByStudentAsync(int studentId)
    {
        return await AttendanceQuery()
            .Where(a => a.StudentId == studentId)
            .OrderBy(a => a.Week.StartDate)
            .ThenBy(a => a.Day)
            .ThenBy(a => a.Para)
            .ToListAsync();
    }

    public async Task<List<Discipline>> GetDisciplinesByTeacherAsync(int teacherId)
    {
        return await DisciplineQuery()
            .Where(d => d.TeacherId == teacherId)
            .OrderBy(d => d.Subject.Name)
            .ThenBy(d => d.Group.Name)
            .ToListAsync();
    }

    public async Task<List<Schedule>> GetSchedulesByTeacherAsync(int teacherId)
    {
        return await ScheduleQuery()
            .Where(s => s.TeacherId == teacherId)
            .OrderBy(s => s.Week.StartDate)
            .ThenBy(s => s.Den)
            .ThenBy(s => s.Para)
            .ToListAsync();
    }

    public async Task<List<Attendance>> GetAttendancesByTeacherAsync(int teacherId)
    {
        return await AttendanceQuery()
            .Where(a => _context.Schedules
                .AsNoTracking()
                .Any(s =>
                    s.TeacherId == teacherId &&
                    s.WeekId == a.WeekId &&
                    s.Den == a.Day &&
                    s.Para == a.Para &&
                    a.Student.GroupId.HasValue &&
                    s.GroupId == a.Student.GroupId.Value))
            .OrderBy(a => a.Week.StartDate)
            .ThenBy(a => a.Day)
            .ThenBy(a => a.Para)
            .ThenBy(a => a.Student.LastName)
            .ThenBy(a => a.Student.FirstName)
            .ToListAsync();
    }

    public async Task<List<AcademicPerformance>> GetAcademicPerformancesByTeacherAsync(int teacherId)
    {
        return await AcademicPerformanceQuery()
            .Where(ap => ap.TeacherId == teacherId)
            .OrderBy(ap => ap.Student.LastName)
            .ThenBy(ap => ap.Student.FirstName)
            .ThenBy(ap => ap.Discipline.Subject.Name)
            .ToListAsync();
    }

    public async Task<List<Execution>> GetExecutionsByTeacherAsync(int teacherId)
    {
        return await _context.Executions
            .AsNoTracking()
            .Include(e => e.Schedule)
            .ThenInclude(s => s.Discipline)
            .ThenInclude(d => d.Subject)
            .Include(e => e.Schedule)
            .ThenInclude(s => s.Group)
            .Where(e => e.Schedule.TeacherId == teacherId)
            .OrderByDescending(e => e.ExecutionDate)
            .ThenBy(e => e.Schedule.Den)
            .ThenBy(e => e.Schedule.Para)
            .ToListAsync();
    }

    private IQueryable<Schedule> ScheduleQuery()
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

    private IQueryable<Discipline> DisciplineQuery()
    {
        return _context.Disciplines
            .AsNoTracking()
            .Include(d => d.Subject)
            .Include(d => d.Teacher)
            .Include(d => d.Group);
    }

    private IQueryable<Attendance> AttendanceQuery()
    {
        return _context.Attendances
            .AsNoTracking()
            .Include(a => a.Student)
            .Include(a => a.Week);
    }

    private IQueryable<AcademicPerformance> AcademicPerformanceQuery()
    {
        return _context.AcademicPerformances
            .AsNoTracking()
            .Include(ap => ap.Student)
            .Include(ap => ap.Discipline)
            .ThenInclude(d => d.Subject)
            .Include(ap => ap.Teacher);
    }
}
