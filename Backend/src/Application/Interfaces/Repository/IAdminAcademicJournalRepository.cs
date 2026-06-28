using Application.DTOs.AdminAcademicJournalDTOs;

namespace Application.Interfaces.Repository;

public interface IAdminAcademicJournalRepository
{
    Task<List<AcademicFacultyCardDto>> GetFacultiesAsync();
    Task<bool> FacultyExistsAsync(int facultyId);
    Task<List<AcademicGroupCardDto>> GetGroupsByFacultyAsync(int facultyId);
    Task<bool> GroupExistsAsync(int groupId);
    Task<List<AcademicStudentCardDto>> GetStudentsByGroupAsync(int groupId);
    Task<StudentGradebookDto?> GetStudentGradebookAsync(int studentId);
    Task<StudentJournalDto?> GetStudentJournalAsync(int studentId);
}
