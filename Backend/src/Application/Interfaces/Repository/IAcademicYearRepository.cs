using Domain.Entities;

namespace Application.Interfaces.Repository;

public interface IAcademicYearRepository
{
    Task<AcademicYear?> GetByIdAsync(int id);
    Task<AcademicYear?> GetCurrentAsync();
    Task<List<AcademicYear>> GetAllAsync();
    Task<bool> NameExistsAsync(string name, int? excludedId = null);
    Task AddAsync(AcademicYear academicYear);
    Task UpdateAsync(AcademicYear academicYear);
    Task DeleteAsync(AcademicYear academicYear);
}
