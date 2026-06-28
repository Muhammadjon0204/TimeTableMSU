using Application.Common;
using Application.DTOs.ScheduleBoardDTOs;

namespace Application.Interfaces.Interface;

public interface IAdminScheduleBoardService
{
    Task<Result<WeeklyScheduleBoardDto>> GetWeeklyAsync(int? weekId, int? groupId, int? teacherId, int? audienceId);
    Task<Result<List<WeekLookupDto>>> GetWeekLookupsAsync();
    Task<Result<List<LookupDto>>> GetGroupLookupsAsync();
    Task<Result<List<LookupDto>>> GetTeacherLookupsAsync();
    Task<Result<List<LookupDto>>> GetAudienceLookupsAsync();
}
