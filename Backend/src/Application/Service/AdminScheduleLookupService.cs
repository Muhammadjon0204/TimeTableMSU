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
            DisplayName = BuildWeekDisplayName(week),
            AcademicYearName = week.AcademicYear?.Name ?? string.Empty,
            PeriodName = week.AcademicPeriod?.Name ?? string.Empty,
            PeriodType = week.AcademicPeriod?.Type.ToString() ?? string.Empty,
            WeekType = week.Type.ToString(),
            IsCurrent = week.IsCurrent || (week.StartDate.Date <= DateTime.Today && week.EndDate.Date >= DateTime.Today)
        }).ToList();

        return Result<List<WeekLookupDto>>.Success(result);
    }

    public async Task<Result<List<AcademicPeriodLookupDto>>> GetAcademicPeriodsAsync()
    {
        List<AcademicPeriod> periods = await _repository.GetAcademicPeriodsAsync();

        List<AcademicPeriodLookupDto> result = periods.Select(period => new AcademicPeriodLookupDto
        {
            Id = period.Id,
            Name = period.Name,
            Type = period.Type.ToString(),
            StartDate = period.StartDate,
            EndDate = period.EndDate,
            AcademicYearName = period.AcademicYear.Name
        }).ToList();

        return Result<List<AcademicPeriodLookupDto>>.Success(result);
    }

    public async Task<Result<List<HolidayLookupDto>>> GetHolidaysAsync()
    {
        List<Holiday> holidays = await _repository.GetHolidaysAsync();

        List<HolidayLookupDto> result = holidays.Select(holiday => new HolidayLookupDto
        {
            Id = holiday.Id,
            Name = holiday.Name,
            Date = holiday.Date,
            IsStudyBlocked = holiday.IsStudyBlocked,
            Note = holiday.Note
        }).ToList();

        return Result<List<HolidayLookupDto>>.Success(result);
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
            DisplayName = $"{subject.Name} · {subject.Semester} semester · {subject.ControlForm}"
        }).ToList();

        return Result<List<SubjectLookupDto>>.Success(result);
    }

    public async Task<Result<List<DisciplineScheduleOptionDto>>> GetDisciplineOptionsAsync(int subjectId)
    {
        if (subjectId <= 0)
        {
            return Result<List<DisciplineScheduleOptionDto>>.Failure("Invalid subject id");
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

    private static string BuildWeekDisplayName(Weeks week)
    {
        string prefix = string.IsNullOrWhiteSpace(week.AcademicPeriod?.Name)
            ? week.Type.ToString()
            : week.AcademicPeriod.Name;

        return $"{prefix} · {week.Name} · {week.StartDate:dd.MM.yyyy} - {week.EndDate:dd.MM.yyyy}";
    }
}
