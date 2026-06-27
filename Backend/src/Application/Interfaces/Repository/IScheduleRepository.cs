// src/Application/Interfaces/Repository/IScheduleRepository.cs
using Application.Common;
using Domain.Entities;

namespace Application.Interfaces.Repository;

public interface IScheduleRepository
{
    Task<Schedule?> GetByIdAsync(int id);
    Task<List<Schedule>> GetAllAsync();
    Task<PagedResult<Schedule>> GetPagedAsync(int pageNumber, int pageSize);
    Task<List<Schedule>> GetByGroupAndWeekAsync(int groupId, int weekId);
    Task<List<Schedule>> GetByTeacherAndWeekAsync(int teacherId, int weekId);
    Task<bool> HasCollisionAsync(short den, short para, int weekId, int? audienceId, int? groupId, int? teacherId, int? excludedId = null);
    Task AddAsync(Schedule schedule);
    Task UpdateAsync(Schedule schedule);
    Task DeleteAsync(Schedule schedule);
}
