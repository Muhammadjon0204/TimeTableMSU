using Application.Interfaces.Repository;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class AdminScheduleLookupRepository : IAdminScheduleLookupRepository
{
    private readonly AppDbContext _context;

    public AdminScheduleLookupRepository(AppDbContext context)
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

    public async Task<List<Subject>> GetSubjectsWithDisciplinesAsync()
    {
        return await _context.Subjects
            .AsNoTracking()
            .Where(subject => subject.Disciplines.Any())
            .OrderBy(subject => subject.Name)
            .ThenBy(subject => subject.Semester)
            .ToListAsync();
    }

    public async Task<List<Discipline>> GetDisciplineOptionsAsync(int subjectId)
    {
        return await _context.Disciplines
            .AsNoTracking()
            .Include(discipline => discipline.Subject)
            .Include(discipline => discipline.Teacher)
            .Include(discipline => discipline.Group)
            .ThenInclude(group => group.Speciality)
            .ThenInclude(speciality => speciality.Faculty)
            .Where(discipline => discipline.SubjectId == subjectId)
            .OrderBy(discipline => discipline.Teacher.LastName)
            .ThenBy(discipline => discipline.Teacher.FirstName)
            .ThenBy(discipline => discipline.Group.Name)
            .ToListAsync();
    }

    public async Task<List<Audience>> GetAudiencesAsync()
    {
        return await _context.Audiences
            .AsNoTracking()
            .OrderBy(audience => audience.Number)
            .ToListAsync();
    }
}
