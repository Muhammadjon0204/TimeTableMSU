using Application.Interfaces.Repository;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class GroupRepository : IGroupRepository
{
    private readonly AppDbContext _context;

    public GroupRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Group?> GetByIdAsync(int id)
    {
        return await _context.Groups
            .AsNoTracking()
            .Include(g => g.Speciality)
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task<List<Group>> GetAllAsync()
    {
        return await _context.Groups
            .AsNoTracking()
            .Include(g => g.Speciality)
            .OrderBy(g => g.Name)
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(string name, int specialityId, int? excludedId = null)
    {
        string normalizedName = name.Trim().ToLower();

        IQueryable<Group> query = _context.Groups
            .AsNoTracking()
            .Where(g => g.SpecialityId == specialityId);

        if (excludedId.HasValue)
        {
            query = query.Where(g => g.Id != excludedId.Value);
        }

        return await query.AnyAsync(g => g.Name.ToLower() == normalizedName);
    }

    public async Task AddAsync(Group group)
    {
        await _context.Groups.AddAsync(group);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Group group)
    {
        _context.Groups.Update(group);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Group group)
    {
        _context.Groups.Remove(group);
        await _context.SaveChangesAsync();
    }
}
