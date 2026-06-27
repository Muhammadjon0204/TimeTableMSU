// src/Application/Interfaces/Repository/IAcademicPerformanceRepository.cs
using Application.Common;
using Domain.Entities;

namespace Application.Interfaces.Repository;

public interface IAcademicPerformanceRepository
{
    Task<AcademicPerformance?> GetByIdAsync(int id);
    Task<List<AcademicPerformance>> GetAllAsync();
    Task<PagedResult<AcademicPerformance>> GetPagedAsync(int pageNumber, int pageSize);
    Task<bool> ExistsAsync(int studentId, int disciplineId, int teacherId, int? excludedId = null);
    Task AddAsync(AcademicPerformance academicPerformance);
    Task UpdateAsync(AcademicPerformance academicPerformance);
    Task DeleteAsync(AcademicPerformance academicPerformance);
}
