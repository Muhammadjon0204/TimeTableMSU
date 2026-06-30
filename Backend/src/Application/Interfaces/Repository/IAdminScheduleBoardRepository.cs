using Domain.Entities;

namespace Application.Interfaces.Repository;

public interface IAdminScheduleBoardRepository
{
    Task<List<Weeks>> GetWeeksAsync();
    Task<List<Group>> GetGroupsAsync();
    Task<List<Teacher>> GetTeachersAsync();
    Task<List<Audience>> GetAudiencesAsync();
    Task<List<Schedule>> GetWeeklySchedulesAsync(int weekId, int? groupId, int? teacherId, int? audienceId);
    Task<List<Holiday>> GetHolidaysAsync(DateTime startDate, DateTime endDate);
}
