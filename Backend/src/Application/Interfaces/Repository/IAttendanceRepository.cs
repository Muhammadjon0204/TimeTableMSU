// src/Application/Interfaces/Repository/IAttendanceRepository.cs
using Application.Common;
using Domain.Entities;

namespace Application.Interfaces.Repository;

public interface IAttendanceRepository
{
    Task<Attendance?> GetByIdAsync(int id);
    Task<List<Attendance>> GetAllAsync();
    Task<PagedResult<Attendance>> GetPagedAsync(int pageNumber, int pageSize);
    Task<bool> ExistsDuplicateAsync(int studentId, int weekId, short day, short para, int? excludedId = null);
    Task AddAsync(Attendance attendance);
    Task UpdateAsync(Attendance attendance);
    Task DeleteAsync(Attendance attendance);
}
