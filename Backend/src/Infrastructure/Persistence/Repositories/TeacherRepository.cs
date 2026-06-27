using Application.Interfaces.Repository;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class TeacherRepository : ITeacherRepository
{
    private readonly AppDbContext _context;

    public TeacherRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Teacher?> GetByIdAsync(int id)
    {
        return await _context.Teachers
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<List<Teacher>> GetAllAsync()
    {
        return await _context.Teachers
            .AsNoTracking()
            .OrderBy(t => t.LastName)
            .ThenBy(t => t.FirstName)
            .ToListAsync();
    }

    public async Task<bool> ExistsByEmailAsync(string email, int? excludedId = null)
    {
        string normalizedEmail = email.Trim().ToLower();

        IQueryable<Teacher> query = _context.Teachers.AsNoTracking();

        if (excludedId.HasValue)
        {
            query = query.Where(t => t.Id != excludedId.Value);
        }

        return await query.AnyAsync(t => t.Email != null && t.Email.ToLower() == normalizedEmail);
    }

    public async Task<bool> ExistsByFullNameAsync(string firstName, string lastName, string? fatherName, int? excludedId = null)
    {
        string normalizedFirstName = firstName.Trim().ToLower();
        string normalizedLastName = lastName.Trim().ToLower();
        string normalizedFatherName = string.IsNullOrWhiteSpace(fatherName) ? string.Empty : fatherName.Trim().ToLower();

        IQueryable<Teacher> query = _context.Teachers.AsNoTracking();

        if (excludedId.HasValue)
        {
            query = query.Where(t => t.Id != excludedId.Value);
        }

        return await query.AnyAsync(t =>
            t.FirstName.ToLower() == normalizedFirstName &&
            t.LastName.ToLower() == normalizedLastName &&
            ((t.FatherName ?? string.Empty).ToLower() == normalizedFatherName));
    }

    public async Task AddAsync(Teacher teacher)
    {
        await _context.Teachers.AddAsync(teacher);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Teacher teacher)
    {
        _context.Teachers.Update(teacher);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Teacher teacher)
    {
        _context.Teachers.Remove(teacher);
        await _context.SaveChangesAsync();
    }
}
