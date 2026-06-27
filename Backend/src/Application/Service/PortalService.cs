using Application.Common;
using Application.DTOs.PortalDTOs;
using Application.Interfaces.Interface;
using Application.Interfaces.Repository;
using Domain.Entities;

namespace Application.Service;

public class PortalService : IPortalService
{
    private readonly IPortalRepository _portalRepository;

    public PortalService(IPortalRepository portalRepository)
    {
        _portalRepository = portalRepository;
    }

    public async Task<Result<PortalStudentProfileDto>> GetStudentProfileAsync(string email)
    {
        Result<Student> studentResult = await GetStudentByEmailAsync(email);

        if (studentResult.IsFailure)
        {
            return Result<PortalStudentProfileDto>.Failure(studentResult.Error);
        }

        return Result<PortalStudentProfileDto>.Success(MapStudent(studentResult.Value));
    }

    public async Task<Result<List<PortalScheduleDto>>> GetStudentScheduleAsync(string email)
    {
        Result<Student> studentResult = await GetStudentByEmailAsync(email);

        if (studentResult.IsFailure)
        {
            return Result<List<PortalScheduleDto>>.Failure(studentResult.Error);
        }

        if (!studentResult.Value.GroupId.HasValue)
        {
            return Result<List<PortalScheduleDto>>.Success(new List<PortalScheduleDto>());
        }

        List<Schedule> schedules = await _portalRepository.GetSchedulesByGroupAsync(studentResult.Value.GroupId.Value);
        List<PortalScheduleDto> mapped = schedules.Select(MapSchedule).ToList();

        return Result<List<PortalScheduleDto>>.Success(mapped);
    }

    public async Task<Result<List<PortalAcademicPerformanceDto>>> GetStudentMarksAsync(string email)
    {
        Result<Student> studentResult = await GetStudentByEmailAsync(email);

        if (studentResult.IsFailure)
        {
            return Result<List<PortalAcademicPerformanceDto>>.Failure(studentResult.Error);
        }

        List<AcademicPerformance> marks = await _portalRepository.GetAcademicPerformancesByStudentAsync(studentResult.Value.Id);
        List<PortalAcademicPerformanceDto> mapped = marks.Select(MapAcademicPerformance).ToList();

        return Result<List<PortalAcademicPerformanceDto>>.Success(mapped);
    }

    public async Task<Result<List<PortalAttendanceDto>>> GetStudentAttendancesAsync(string email)
    {
        Result<Student> studentResult = await GetStudentByEmailAsync(email);

        if (studentResult.IsFailure)
        {
            return Result<List<PortalAttendanceDto>>.Failure(studentResult.Error);
        }

        List<Attendance> attendances = await _portalRepository.GetAttendancesByStudentAsync(studentResult.Value.Id);
        List<PortalAttendanceDto> mapped = attendances.Select(MapAttendance).ToList();

        return Result<List<PortalAttendanceDto>>.Success(mapped);
    }

    public async Task<Result<PortalTeacherProfileDto>> GetTeacherProfileAsync(string email)
    {
        Result<Teacher> teacherResult = await GetTeacherByEmailAsync(email);

        if (teacherResult.IsFailure)
        {
            return Result<PortalTeacherProfileDto>.Failure(teacherResult.Error);
        }

        return Result<PortalTeacherProfileDto>.Success(MapTeacher(teacherResult.Value));
    }

    public async Task<Result<List<PortalScheduleDto>>> GetTeacherScheduleAsync(string email)
    {
        Result<Teacher> teacherResult = await GetTeacherByEmailAsync(email);

        if (teacherResult.IsFailure)
        {
            return Result<List<PortalScheduleDto>>.Failure(teacherResult.Error);
        }

        List<Schedule> schedules = await _portalRepository.GetSchedulesByTeacherAsync(teacherResult.Value.Id);
        List<PortalScheduleDto> mapped = schedules.Select(MapSchedule).ToList();

        return Result<List<PortalScheduleDto>>.Success(mapped);
    }

    public async Task<Result<List<PortalDisciplineDto>>> GetTeacherDisciplinesAsync(string email)
    {
        Result<Teacher> teacherResult = await GetTeacherByEmailAsync(email);

        if (teacherResult.IsFailure)
        {
            return Result<List<PortalDisciplineDto>>.Failure(teacherResult.Error);
        }

        List<Discipline> disciplines = await _portalRepository.GetDisciplinesByTeacherAsync(teacherResult.Value.Id);
        List<PortalDisciplineDto> mapped = disciplines.Select(MapDiscipline).ToList();

        return Result<List<PortalDisciplineDto>>.Success(mapped);
    }

    public async Task<Result<List<PortalAttendanceDto>>> GetTeacherAttendancesAsync(string email)
    {
        Result<Teacher> teacherResult = await GetTeacherByEmailAsync(email);

        if (teacherResult.IsFailure)
        {
            return Result<List<PortalAttendanceDto>>.Failure(teacherResult.Error);
        }

        List<Attendance> attendances = await _portalRepository.GetAttendancesByTeacherAsync(teacherResult.Value.Id);
        List<PortalAttendanceDto> mapped = attendances.Select(MapAttendance).ToList();

        return Result<List<PortalAttendanceDto>>.Success(mapped);
    }

    public async Task<Result<List<PortalAcademicPerformanceDto>>> GetTeacherAcademicPerformancesAsync(string email)
    {
        Result<Teacher> teacherResult = await GetTeacherByEmailAsync(email);

        if (teacherResult.IsFailure)
        {
            return Result<List<PortalAcademicPerformanceDto>>.Failure(teacherResult.Error);
        }

        List<AcademicPerformance> performances = await _portalRepository.GetAcademicPerformancesByTeacherAsync(teacherResult.Value.Id);
        List<PortalAcademicPerformanceDto> mapped = performances.Select(MapAcademicPerformance).ToList();

        return Result<List<PortalAcademicPerformanceDto>>.Success(mapped);
    }

    public async Task<Result<List<PortalExecutionDto>>> GetTeacherExecutionsAsync(string email)
    {
        Result<Teacher> teacherResult = await GetTeacherByEmailAsync(email);

        if (teacherResult.IsFailure)
        {
            return Result<List<PortalExecutionDto>>.Failure(teacherResult.Error);
        }

        List<Execution> executions = await _portalRepository.GetExecutionsByTeacherAsync(teacherResult.Value.Id);
        List<PortalExecutionDto> mapped = executions.Select(MapExecution).ToList();

        return Result<List<PortalExecutionDto>>.Success(mapped);
    }

    private async Task<Result<Student>> GetStudentByEmailAsync(string email)
    {
        Result<string> normalizedEmailResult = NormalizeEmail(email);

        if (normalizedEmailResult.IsFailure)
        {
            return Result<Student>.Failure(normalizedEmailResult.Error);
        }

        Student? student = await _portalRepository.GetStudentByEmailAsync(normalizedEmailResult.Value);

        if (student == null)
        {
            return Result<Student>.Failure("Student profile was not found for current user email");
        }

        return Result<Student>.Success(student);
    }

    private async Task<Result<Teacher>> GetTeacherByEmailAsync(string email)
    {
        Result<string> normalizedEmailResult = NormalizeEmail(email);

        if (normalizedEmailResult.IsFailure)
        {
            return Result<Teacher>.Failure(normalizedEmailResult.Error);
        }

        Teacher? teacher = await _portalRepository.GetTeacherByEmailAsync(normalizedEmailResult.Value);

        if (teacher == null)
        {
            return Result<Teacher>.Failure("Teacher profile was not found for current user email");
        }

        return Result<Teacher>.Success(teacher);
    }

    private static Result<string> NormalizeEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return Result<string>.Failure("Current user email claim is missing");
        }

        return Result<string>.Success(email.Trim().ToLowerInvariant());
    }

    private static PortalStudentProfileDto MapStudent(Student student)
    {
        return new PortalStudentProfileDto
        {
            Id = student.Id,
            FullName = BuildFullName(student.LastName, student.FirstName, student.FatherName),
            FirstName = student.FirstName,
            LastName = student.LastName,
            FatherName = student.FatherName,
            Email = student.Email,
            GroupId = student.GroupId,
            GroupName = student.Group?.Name
        };
    }

    private static PortalTeacherProfileDto MapTeacher(Teacher teacher)
    {
        return new PortalTeacherProfileDto
        {
            Id = teacher.Id,
            FullName = BuildFullName(teacher.LastName, teacher.FirstName, teacher.FatherName),
            FirstName = teacher.FirstName,
            LastName = teacher.LastName,
            FatherName = teacher.FatherName,
            Email = teacher.Email,
            TeacherDegree = teacher.TeacherDegree,
            TeacherTitle = teacher.TeacherTitle,
            TeacherPost = teacher.TeacherPost
        };
    }

    private static PortalScheduleDto MapSchedule(Schedule schedule)
    {
        return new PortalScheduleDto
        {
            Id = schedule.Id,
            Den = schedule.Den,
            Para = schedule.Para,
            SubjectName = schedule.Discipline.Subject.Name,
            TeacherFullName = BuildFullName(schedule.Teacher.LastName, schedule.Teacher.FirstName, schedule.Teacher.FatherName),
            AudienceNumber = schedule.Audience.Number,
            GroupName = schedule.Group.Name,
            WeekName = schedule.Week.Name,
            LectureType = schedule.LectureType?.ToString() ?? string.Empty
        };
    }

    private static PortalDisciplineDto MapDiscipline(Discipline discipline)
    {
        return new PortalDisciplineDto
        {
            Id = discipline.Id,
            SubjectName = discipline.Subject.Name,
            TeacherFullName = BuildFullName(discipline.Teacher.LastName, discipline.Teacher.FirstName, discipline.Teacher.FatherName),
            GroupName = discipline.Group.Name,
            LectureHourCount = discipline.LectureHourCount,
            PracticeHourCount = discipline.PracticeHourCount,
            LaboratoryHourCount = discipline.LaboratoryHourCount,
            OtherHourCount = discipline.OtherHourCount
        };
    }

    private static PortalAttendanceDto MapAttendance(Attendance attendance)
    {
        return new PortalAttendanceDto
        {
            Id = attendance.Id,
            Day = attendance.Day,
            Para = attendance.Para,
            Mark = attendance.Mark,
            StudentFullName = BuildFullName(attendance.Student.LastName, attendance.Student.FirstName, attendance.Student.FatherName),
            WeekName = attendance.Week.Name
        };
    }

    private static PortalAcademicPerformanceDto MapAcademicPerformance(AcademicPerformance academicPerformance)
    {
        return new PortalAcademicPerformanceDto
        {
            Id = academicPerformance.Id,
            StudentFullName = BuildFullName(academicPerformance.Student.LastName, academicPerformance.Student.FirstName, academicPerformance.Student.FatherName),
            SubjectName = academicPerformance.Discipline.Subject.Name,
            TeacherFullName = BuildFullName(academicPerformance.Teacher.LastName, academicPerformance.Teacher.FirstName, academicPerformance.Teacher.FatherName),
            ControlForm = academicPerformance.ControlForm,
            Tur = academicPerformance.Tur,
            Mark = academicPerformance.Mark
        };
    }

    private static PortalExecutionDto MapExecution(Execution execution)
    {
        string scheduleInfo = $"{execution.Schedule.Discipline.Subject.Name}, {execution.Schedule.Group.Name}, day {execution.Schedule.Den}, pair {execution.Schedule.Para}";

        return new PortalExecutionDto
        {
            Id = execution.Id,
            ScheduleId = execution.ScheduleId,
            ScheduleInfo = scheduleInfo,
            ExecutionDate = execution.ExecutionDate,
            Status = execution.Status,
            Notes = execution.Notes
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
