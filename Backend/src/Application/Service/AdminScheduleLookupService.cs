using Application.Common;
using Application.DTOs.ScheduleLookupDTOs;
using Application.Interfaces.Interface;
using Application.Interfaces.Repository;
using Domain.Entities;

namespace Application.Service;

public class AdminScheduleLookupService : IAdminScheduleLookupService
{
    private readonly IAdminScheduleLookupRepository _repository;

    public AdminScheduleLookupService(IAdminScheduleLookupRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<WeekLookupDto>>> GetWeeksAsync()
    {
        List<Weeks> weeks = await _repository.GetWeeksAsync();

        List<WeekLookupDto> result = weeks.Select(week => new WeekLookupDto
        {
            Id = week.Id,
            Name = week.Name,
            StartDate = week.StartDate,
            EndDate = week.EndDate,
            DisplayName = $"{week.Name} · {week.StartDate:dd.MM.yyyy} - {week.EndDate:dd.MM.yyyy}"
        }).ToList();

        return Result<List<WeekLookupDto>>.Success(result);
    }

    public async Task<Result<List<SubjectLookupDto>>> GetDisciplinesAsync()
    {
        List<Subject> subjects = await _repository.GetSubjectsWithDisciplinesAsync();

        List<SubjectLookupDto> result = subjects.Select(subject => new SubjectLookupDto
        {
            SubjectId = subject.Id,
            SubjectName = subject.Name,
            Semester = subject.Semester,
            ControlForm = subject.ControlForm.ToString(),
            DisplayName = $"{subject.Name} · {subject.Semester} семестр · {subject.ControlForm}"
        }).ToList();

        return Result<List<SubjectLookupDto>>.Success(result);
    }

    public async Task<Result<List<DisciplineScheduleOptionDto>>> GetDisciplineOptionsAsync(int subjectId)
    {
        if (subjectId <= 0)
        {
            return Result<List<DisciplineScheduleOptionDto>>.Failure("Некорректный идентификатор дисциплины");
        }

        List<Discipline> disciplines = await _repository.GetDisciplineOptionsAsync(subjectId);

        List<DisciplineScheduleOptionDto> result = disciplines.Select(discipline => new DisciplineScheduleOptionDto
        {
            DisciplineId = discipline.Id,
            SubjectId = discipline.SubjectId,
            SubjectName = discipline.Subject.Name,
            TeacherId = discipline.TeacherId,
            TeacherFullName = BuildFullName(discipline.Teacher.LastName, discipline.Teacher.FirstName, discipline.Teacher.FatherName),
            GroupId = discipline.GroupId,
            GroupName = discipline.Group.Name,
            SpecialityId = discipline.Group.SpecialityId,
            SpecialityName = discipline.Group.Speciality.Name,
            FacultyId = discipline.Group.Speciality.FacultyId,
            FacultyName = discipline.Group.Speciality.Faculty.Name,
            Semester = discipline.Subject.Semester
        }).ToList();

        return Result<List<DisciplineScheduleOptionDto>>.Success(result);
    }

    public async Task<Result<List<AudienceLookupDto>>> GetAudiencesAsync(string? lectureType)
    {
        List<Audience> audiences = await _repository.GetAudiencesAsync();
        string normalizedLectureType = lectureType?.Trim() ?? string.Empty;

        List<AudienceLookupDto> result = audiences
            .Where(audience => MatchesLectureType(audience.Type.ToString(), normalizedLectureType))
            .Select(audience => new AudienceLookupDto
            {
                Id = audience.Id,
                Number = audience.Number,
                Type = audience.Type.ToString(),
                DisplayName = $"{audience.Number} · {audience.Type}"
            })
            .ToList();

        return Result<List<AudienceLookupDto>>.Success(result);
    }

    private static bool MatchesLectureType(string audienceType, string lectureType)
    {
        if (string.IsNullOrWhiteSpace(lectureType))
        {
            return true;
        }

        return lectureType switch
        {
            "Lecture" => audienceType is "LectureRoom" or "LectureHall" or "StreamHall" or "GeneralHall",
            "Practice" => audienceType is "LectureRoom" or "GeneralHall" or "ComputerLab",
            "Laboratory" => audienceType.Contains("Lab", StringComparison.OrdinalIgnoreCase) || audienceType == "ComputerLab",
            _ => true
        };
    }

    private static string BuildFullName(string lastName, string firstName, string? fatherName)
    {
        if (string.IsNullOrWhiteSpace(fatherName))
        {
            return $"{lastName} {firstName}";
        }

        return $"{lastName} {firstName} {fatherName}";
    }
}
