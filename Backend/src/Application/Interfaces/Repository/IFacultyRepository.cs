using Domain.Entities;

namespace Application.Interfaces.Repository;

public interface IFacultyRepository
{
    Task<Faculty?> GetByIdAsync(int id);
    Task<List<Faculty>> GetAllAsync();
    Task<bool> ExistsByNameAsync(string name, int? excludedId = null);
    Task AddAsync(Faculty faculty);
    Task UpdateAsync(Faculty faculty);
    Task DeleteAsync(Faculty faculty);
}
