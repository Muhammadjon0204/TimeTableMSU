using Application.Common;
using Application.DTOs.AdminAcademicJournalDTOs;

namespace Application.Interfaces.Interface;

public interface IAdminAcademicJournalService
{
    Task<Result<List<AcademicFacultyCardDto>>> GetFacultiesAsync();
    Task<Result<List<AcademicGroupCardDto>>> GetGroupsByFacultyAsync(int facultyId);
    Task<Result<List<AcademicStudentCardDto>>> GetStudentsByGroupAsync(int groupId);
    Task<Result<StudentGradebookDto>> GetStudentGradebookAsync(int studentId);
    Task<Result<StudentJournalDto>> GetStudentJournalAsync(int studentId);
}
