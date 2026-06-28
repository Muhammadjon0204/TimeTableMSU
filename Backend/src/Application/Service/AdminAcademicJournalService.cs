using Application.Common;
using Application.DTOs.AdminAcademicJournalDTOs;
using Application.Interfaces.Interface;
using Application.Interfaces.Repository;

namespace Application.Service;

public class AdminAcademicJournalService : IAdminAcademicJournalService
{
    private readonly IAdminAcademicJournalRepository _repository;

    public AdminAcademicJournalService(IAdminAcademicJournalRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<AcademicFacultyCardDto>>> GetFacultiesAsync()
    {
        return Result<List<AcademicFacultyCardDto>>.Success(await _repository.GetFacultiesAsync());
    }

    public async Task<Result<List<AcademicGroupCardDto>>> GetGroupsByFacultyAsync(int facultyId)
    {
        if (facultyId <= 0)
        {
            return Result<List<AcademicGroupCardDto>>.Failure("Invalid faculty id");
        }

        if (!await _repository.FacultyExistsAsync(facultyId))
        {
            return Result<List<AcademicGroupCardDto>>.Failure("Faculty not found");
        }

        return Result<List<AcademicGroupCardDto>>.Success(await _repository.GetGroupsByFacultyAsync(facultyId));
    }

    public async Task<Result<List<AcademicStudentCardDto>>> GetStudentsByGroupAsync(int groupId)
    {
        if (groupId <= 0)
        {
            return Result<List<AcademicStudentCardDto>>.Failure("Invalid group id");
        }

        if (!await _repository.GroupExistsAsync(groupId))
        {
            return Result<List<AcademicStudentCardDto>>.Failure("Group not found");
        }

        return Result<List<AcademicStudentCardDto>>.Success(await _repository.GetStudentsByGroupAsync(groupId));
    }

    public async Task<Result<StudentGradebookDto>> GetStudentGradebookAsync(int studentId)
    {
        if (studentId <= 0)
        {
            return Result<StudentGradebookDto>.Failure("Invalid student id");
        }

        StudentGradebookDto? gradebook = await _repository.GetStudentGradebookAsync(studentId);

        if (gradebook == null)
        {
            return Result<StudentGradebookDto>.Failure("Student not found");
        }

        return Result<StudentGradebookDto>.Success(gradebook);
    }

    public async Task<Result<StudentJournalDto>> GetStudentJournalAsync(int studentId)
    {
        if (studentId <= 0)
        {
            return Result<StudentJournalDto>.Failure("Invalid student id");
        }

        StudentJournalDto? journal = await _repository.GetStudentJournalAsync(studentId);

        if (journal == null)
        {
            return Result<StudentJournalDto>.Failure("Student not found");
        }

        return Result<StudentJournalDto>.Success(journal);
    }
}
