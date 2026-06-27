using Application.Common;
using Application.DTOs.PortalDTOs;

namespace Application.Interfaces.Interface;

public interface IPortalService
{
    Task<Result<PortalStudentProfileDto>> GetStudentProfileAsync(string email);
    Task<Result<List<PortalScheduleDto>>> GetStudentScheduleAsync(string email);
    Task<Result<List<PortalAcademicPerformanceDto>>> GetStudentMarksAsync(string email);
    Task<Result<List<PortalAttendanceDto>>> GetStudentAttendancesAsync(string email);
    Task<Result<PortalTeacherProfileDto>> GetTeacherProfileAsync(string email);
    Task<Result<List<PortalScheduleDto>>> GetTeacherScheduleAsync(string email);
    Task<Result<List<PortalDisciplineDto>>> GetTeacherDisciplinesAsync(string email);
    Task<Result<List<PortalAttendanceDto>>> GetTeacherAttendancesAsync(string email);
    Task<Result<List<PortalAcademicPerformanceDto>>> GetTeacherAcademicPerformancesAsync(string email);
    Task<Result<List<PortalExecutionDto>>> GetTeacherExecutionsAsync(string email);
}
