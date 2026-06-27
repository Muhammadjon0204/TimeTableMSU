using Domain.Entities;

namespace Application.Interfaces.Repository;

public interface ISpecialityRepository
{
    Task<Speciality?> GetByIdAsync(int id);
    Task<List<Speciality>> GetAllAsync();
    Task<bool> ExistsByNameAsync(string name, int facultyId, int? excludedId = null);
    Task AddAsync(Speciality speciality);
    Task UpdateAsync(Speciality speciality);
    Task DeleteAsync(Speciality speciality);
}
