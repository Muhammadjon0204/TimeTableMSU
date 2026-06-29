using Application.Common;
using Application.DTOs.ScheduleLookupDTOs;

namespace Application.Interfaces.Interface;

public interface IAdminScheduleLookupService
{
    Task<Result<List<WeekLookupDto>>> GetWeeksAsync();
    Task<Result<List<SubjectLookupDto>>> GetDisciplinesAsync();
    Task<Result<List<DisciplineScheduleOptionDto>>> GetDisciplineOptionsAsync(int subjectId);
    Task<Result<List<AudienceLookupDto>>> GetAudiencesAsync(string? lectureType);
}
