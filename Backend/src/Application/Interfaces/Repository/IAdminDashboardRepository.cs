using Domain.Entities;

namespace Application.Interfaces.Repository;

public interface IAdminDashboardRepository
{
    Task<Weeks?> GetWeekAsync(int? weekId);
    Task<List<Group>> GetGroupsAsync(int? groupId);
    Task<Dictionary<int, int>> GetStudentCountsByGroupAsync(List<int> groupIds);
    Task<List<Schedule>> GetSchedulesAsync(int weekId, List<int> groupIds);
    Task<List<Attendance>> GetAttendancesAsync(int weekId, List<int> groupIds);
}
