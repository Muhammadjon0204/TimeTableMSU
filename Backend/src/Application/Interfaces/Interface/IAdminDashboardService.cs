using Application.Common;
using Application.DTOs.DashboardDTOs;

namespace Application.Interfaces.Interface;

public interface IAdminDashboardService
{
    Task<Result<AdminAttendanceWeeklyDto>> GetAttendanceWeeklyAsync(int? weekId, int? groupId);
    Task<Result<List<GroupLookupDto>>> GetGroupLookupsAsync();
}
