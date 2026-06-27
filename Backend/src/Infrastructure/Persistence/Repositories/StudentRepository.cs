using Application.Common;
using Application.Interfaces.Repository;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class StudentRepository : IStudentRepository
{
    private readonly AppDbContext _context;

    public StudentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Student?> GetByIdAsync(int id)
    {
        return await _context.Students
            .AsNoTracking()
            .Include(s => s.Group)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<List<Student>> GetAllAsync()
    {
        return await _context.Students
            .AsNoTracking()
            .Include(s => s.Group)
            .OrderBy(s => s.LastName)
            .ThenBy(s => s.FirstName)
            .ToListAsync();
    }

    public async Task<PagedResult<Student>> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm)
    {
        IQueryable<Student> query = _context.Students
            .AsNoTracking()
            .Include(s => s.Group);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            string normalizedSearchTerm = searchTerm.Trim().ToLower();

            query = query.Where(s =>
                s.FirstName.ToLower().Contains(normalizedSearchTerm) ||
                s.LastName.ToLower().Contains(normalizedSearchTerm) ||
                (s.FatherName != null && s.FatherName.ToLower().Contains(normalizedSearchTerm)) ||
                (s.Email != null && s.Email.ToLower().Contains(normalizedSearchTerm)));
        }

        int totalCount = await query.CountAsync();

        List<Student> students = await query
            .OrderBy(s => s.LastName)
            .ThenBy(s => s.FirstName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<Student>(students, totalCount, pageNumber, pageSize);
    }

    public async Task AddAsync(Student student)
    {
        await _context.Students.AddAsync(student);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Student student)
    {
        _context.Students.Update(student);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Student student)
    {
        _context.Students.Remove(student);
        await _context.SaveChangesAsync();
    }
}
