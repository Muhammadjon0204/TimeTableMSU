// src/Application/Interfaces/Repository/IWeekRepository.cs
using Domain.Entities;

namespace Application.Interfaces.Repository;

public interface IWeekRepository
{
    Task<Weeks?> GetByIdAsync(int id);
    Task<List<Weeks>> GetAllAsync();
    Task<List<Weeks>> GetByAcademicYearAsync(int academicYearId);
    Task<bool> HasDateIntersectionAsync(DateTime startDate, DateTime endDate, int? excludedId = null);
    Task<bool> HasDateIntersectionAsync(int academicYearId, DateTime startDate, DateTime endDate, int? excludedId = null);
    Task AddAsync(Weeks week);
    Task AddRangeAsync(IEnumerable<Weeks> weeks);
    Task UpdateAsync(Weeks week);
    Task DeleteAsync(Weeks week);
}
