using Application.Common;
using Application.DTOs.AttendanceDTOs;
using Application.Interfaces.Interface;
using Application.Interfaces.Repository;
using Domain.Entities;
using Domain.Enums;

namespace Application.Service;

public class AttendanceService : IAttendanceService
{
    private readonly IAttendanceRepository _attendanceRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IWeekRepository _weekRepository;

    public AttendanceService(
        IAttendanceRepository attendanceRepository,
        IStudentRepository studentRepository,
        IWeekRepository weekRepository)
    {
        _attendanceRepository = attendanceRepository;
        _studentRepository = studentRepository;
        _weekRepository = weekRepository;
    }

    public async Task<Result<PagedResult<GetAttendanceDto>>> GetPagedAsync(int pageNumber, int pageSize)
    {
        if (pageNumber < 1)
        {
            pageNumber = 1;
        }

        if (pageSize < 1)
        {
            pageSize = 10;
        }

        PagedResult<Attendance> attendances = await _attendanceRepository.GetPagedAsync(pageNumber, pageSize);

        List<GetAttendanceDto> mappedDtos = new List<GetAttendanceDto>();

        foreach (Attendance attendance in attendances.Items)
        {
            GetAttendanceDto mappedDto = await MapToGetAttendanceDtoAsync(attendance);
            mappedDtos.Add(mappedDto);
        }

        PagedResult<GetAttendanceDto> result = new PagedResult<GetAttendanceDto>(
            mappedDtos,
            attendances.TotalCount,
            attendances.PageNumber,
            attendances.PageSize);

        return Result<PagedResult<GetAttendanceDto>>.Success(result);
    }

    public async Task<Result<GetAttendanceDto>> CreateAsync(CreateAttendanceDto dto)
    {
        if (dto == null)
        {
            return Result<GetAttendanceDto>.Failure("Данные запроса отсутствуют");
        }

        Result validationResult = ValidateAttendanceInput(
            dto.StudentId,
            dto.WeekId,
            dto.Day,
            dto.Para,
            dto.Mark);

        if (validationResult.IsFailure)
        {
            return Result<GetAttendanceDto>.Failure(validationResult.Error);
        }

        Student? student = await _studentRepository.GetByIdAsync(dto.StudentId);

        if (student == null)
        {
            return Result<GetAttendanceDto>.Failure("Указанный студент не найден");
        }

        Weeks? week = await _weekRepository.GetByIdAsync(dto.WeekId);

        if (week == null)
        {
            return Result<GetAttendanceDto>.Failure("Указанная неделя не найдена");
        }

        List<Attendance> attendances = await _attendanceRepository.GetAllAsync();

        foreach (Attendance existingAttendance in attendances)
        {
            if (existingAttendance.StudentId == dto.StudentId)
            {
                if (existingAttendance.WeekId == dto.WeekId)
                {
                    if (existingAttendance.Day == dto.Day)
                    {
                        if (existingAttendance.Para == dto.Para)
                        {
                            return Result<GetAttendanceDto>.Failure("Запись посещаемости для этого студента на указанную пару уже существует");
                        }
                    }
                }
            }
        }

        Attendance attendance = new Attendance
        {
            StudentId = dto.StudentId,
            WeekId = dto.WeekId,
            Day = dto.Day,
            Para = dto.Para,
            Mark = dto.Mark,
            Student = student,
            Week = week
        };

        await _attendanceRepository.AddAsync(attendance);

        GetAttendanceDto mappedDto = await MapToGetAttendanceDtoAsync(attendance);

        return Result<GetAttendanceDto>.Success(mappedDto);
    }

    public async Task<Result<GetAttendanceDto>> UpdateAsync(UpdateAttendanceDto dto)
    {
        if (dto == null)
        {
            return Result<GetAttendanceDto>.Failure("Данные запроса отсутствуют");
        }

        if (dto.Id <= 0)
        {
            return Result<GetAttendanceDto>.Failure("Некорректный идентификатор посещаемости");
        }

        Result validationResult = ValidateAttendanceInput(
            dto.StudentId,
            dto.WeekId,
            dto.Day,
            dto.Para,
            dto.Mark);

        if (validationResult.IsFailure)
        {
            return Result<GetAttendanceDto>.Failure(validationResult.Error);
        }

        Attendance? attendance = await _attendanceRepository.GetByIdAsync(dto.Id);

        if (attendance == null)
        {
            return Result<GetAttendanceDto>.Failure("Запись посещаемости не найдена");
        }

        Student? student = await _studentRepository.GetByIdAsync(dto.StudentId);

        if (student == null)
        {
            return Result<GetAttendanceDto>.Failure("Указанный студент не найден");
        }

        Weeks? week = await _weekRepository.GetByIdAsync(dto.WeekId);

        if (week == null)
        {
            return Result<GetAttendanceDto>.Failure("Указанная неделя не найдена");
        }

        List<Attendance> attendances = await _attendanceRepository.GetAllAsync();

        foreach (Attendance existingAttendance in attendances)
        {
            if (existingAttendance.Id != dto.Id)
            {
                if (existingAttendance.StudentId == dto.StudentId)
                {
                    if (existingAttendance.WeekId == dto.WeekId)
                    {
                        if (existingAttendance.Day == dto.Day)
                        {
                            if (existingAttendance.Para == dto.Para)
                            {
                                return Result<GetAttendanceDto>.Failure("Запись посещаемости для этого студента на указанную пару уже существует");
                            }
                        }
                    }
                }
            }
        }

        attendance.StudentId = dto.StudentId;
        attendance.WeekId = dto.WeekId;
        attendance.Day = dto.Day;
        attendance.Para = dto.Para;
        attendance.Mark = dto.Mark;
        attendance.Student = student;
        attendance.Week = week;

        await _attendanceRepository.UpdateAsync(attendance);

        GetAttendanceDto mappedDto = await MapToGetAttendanceDtoAsync(attendance);

        return Result<GetAttendanceDto>.Success(mappedDto);
    }

    public async Task<Result> DeleteAsync(int id)
    {
        if (id <= 0)
        {
            return Result.Failure("Некорректный идентификатор посещаемости");
        }

        Attendance? attendance = await _attendanceRepository.GetByIdAsync(id);

        if (attendance == null)
        {
            return Result.Failure("Запись посещаемости не найдена");
        }

        await _attendanceRepository.DeleteAsync(attendance);

        return Result.Success();
    }

    private static Result ValidateAttendanceInput(
        int studentId,
        int weekId,
        short day,
        short para,
        AttendanceMark? mark)
    {
        if (studentId <= 0)
        {
            return Result.Failure("Некорректный идентификатор студента");
        }

        if (weekId <= 0)
        {
            return Result.Failure("Некорректный идентификатор недели");
        }

        if (day < 1)
        {
            return Result.Failure("Некорректный день недели");
        }

        if (day > 6)
        {
            return Result.Failure("Некорректный день недели");
        }

        if (para < 1)
        {
            return Result.Failure("Некорректный номер пары");
        }

        if (para > 7)
        {
            return Result.Failure("Некорректный номер пары");
        }

        if (mark == null)
        {
            return Result.Failure("Отметка посещаемости обязательна");
        }

        if (!Enum.IsDefined(typeof(AttendanceMark), mark.Value))
        {
            return Result.Failure("Недопустимая отметка посещаемости");
        }

        if (mark.Value != AttendanceMark.Present)
        {
            if (mark.Value != AttendanceMark.Absent)
            {
                if (mark.Value != AttendanceMark.ValidReason)
                {
                    return Result.Failure("Недопустимая отметка посещаемости");
                }
            }
        }

        return Result.Success();
    }

    private async Task<GetAttendanceDto> MapToGetAttendanceDtoAsync(Attendance attendance)
    {
        Student? student = attendance.Student;
        Weeks? week = attendance.Week;

        if (student == null)
        {
            student = await _studentRepository.GetByIdAsync(attendance.StudentId);
        }

        if (week == null)
        {
            week = await _weekRepository.GetByIdAsync(attendance.WeekId);
        }

        string studentFullName = string.Empty;

        if (student != null)
        {
            studentFullName = BuildFullName(student.LastName, student.FirstName, student.FatherName);
        }

        GetAttendanceDto dto = new GetAttendanceDto
        {
            Id = attendance.Id,
            Day = attendance.Day,
            Para = attendance.Para,
            Mark = attendance.Mark,
            StudentFullName = studentFullName,
            WeekName = week?.Name
        };

        return dto;
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
