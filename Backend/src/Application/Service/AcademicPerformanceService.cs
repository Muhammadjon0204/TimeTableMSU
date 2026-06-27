using Application.Common;
using Application.DTOs.AcademicPerformanceDTOs;
using Application.Interfaces.Interface;
using Application.Interfaces.Repository;
using Domain.Entities;

namespace Application.Service;

public class AcademicPerformanceService : IAcademicPerformanceService
{
    private readonly IAcademicPerformanceRepository _academicPerformanceRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IDisciplineRepository _disciplineRepository;
    private readonly ITeacherRepository _teacherRepository;

    public AcademicPerformanceService(
        IAcademicPerformanceRepository academicPerformanceRepository,
        IStudentRepository studentRepository,
        IDisciplineRepository disciplineRepository,
        ITeacherRepository teacherRepository)
    {
        _academicPerformanceRepository = academicPerformanceRepository;
        _studentRepository = studentRepository;
        _disciplineRepository = disciplineRepository;
        _teacherRepository = teacherRepository;
    }

    public async Task<Result<GetAcademicPerformanceDto>> GetByIdAsync(int id)
    {
        if (id <= 0)
        {
            return Result<GetAcademicPerformanceDto>.Failure("Некорректный идентификатор успеваемости");
        }

        AcademicPerformance? academicPerformance = await _academicPerformanceRepository.GetByIdAsync(id);

        if (academicPerformance == null)
        {
            return Result<GetAcademicPerformanceDto>.Failure("Запись успеваемости не найдена");
        }

        GetAcademicPerformanceDto mappedDto = await MapToGetAcademicPerformanceDtoAsync(academicPerformance);

        return Result<GetAcademicPerformanceDto>.Success(mappedDto);
    }

    public async Task<Result<PagedResult<GetAcademicPerformanceDto>>> GetPagedAsync(int pageNumber, int pageSize)
    {
        if (pageNumber < 1)
        {
            pageNumber = 1;
        }

        if (pageSize < 1)
        {
            pageSize = 10;
        }

        PagedResult<AcademicPerformance> academicPerformances = await _academicPerformanceRepository.GetPagedAsync(pageNumber, pageSize);

        List<GetAcademicPerformanceDto> mappedDtos = new List<GetAcademicPerformanceDto>();

        foreach (AcademicPerformance academicPerformance in academicPerformances.Items)
        {
            GetAcademicPerformanceDto mappedDto = await MapToGetAcademicPerformanceDtoAsync(academicPerformance);
            mappedDtos.Add(mappedDto);
        }

        PagedResult<GetAcademicPerformanceDto> result = new PagedResult<GetAcademicPerformanceDto>(
            mappedDtos,
            academicPerformances.TotalCount,
            academicPerformances.PageNumber,
            academicPerformances.PageSize);

        return Result<PagedResult<GetAcademicPerformanceDto>>.Success(result);
    }

    public async Task<Result<GetAcademicPerformanceDto>> CreateAsync(CreateAcademicPerformanceDto dto)
    {
        if (dto == null)
        {
            return Result<GetAcademicPerformanceDto>.Failure("Данные запроса отсутствуют");
        }

        Result validationResult = ValidateAcademicPerformanceInput(dto.StudentId, dto.DisciplineId, dto.TeacherId, dto.Mark);

        if (validationResult.IsFailure)
        {
            return Result<GetAcademicPerformanceDto>.Failure(validationResult.Error);
        }

        Student? student = await _studentRepository.GetByIdAsync(dto.StudentId);

        if (student == null)
        {
            return Result<GetAcademicPerformanceDto>.Failure("Указанный студент не найден");
        }

        Discipline? discipline = await _disciplineRepository.GetByIdAsync(dto.DisciplineId);

        if (discipline == null)
        {
            return Result<GetAcademicPerformanceDto>.Failure("Указанная дисциплина не найдена");
        }

        Teacher? teacher = await _teacherRepository.GetByIdAsync(dto.TeacherId);

        if (teacher == null)
        {
            return Result<GetAcademicPerformanceDto>.Failure("Указанный преподаватель не найден");
        }

        if (discipline.TeacherId != dto.TeacherId)
        {
            return Result<GetAcademicPerformanceDto>.Failure("Преподаватель не ведет указанную дисциплину");
        }

        bool duplicateExists = await _academicPerformanceRepository.ExistsAsync(dto.StudentId, dto.DisciplineId, dto.TeacherId);

        if (duplicateExists)
        {
            return Result<GetAcademicPerformanceDto>.Failure("Оценка для указанного студента по этой дисциплине уже существует");
        }

        AcademicPerformance academicPerformance = new AcademicPerformance
        {
            StudentId = dto.StudentId,
            DisciplineId = dto.DisciplineId,
            TeacherId = dto.TeacherId,
            Mark = dto.Mark
        };

        await _academicPerformanceRepository.AddAsync(academicPerformance);

        academicPerformance.Student = student;
        academicPerformance.Discipline = discipline;
        academicPerformance.Teacher = teacher;

        GetAcademicPerformanceDto mappedDto = await MapToGetAcademicPerformanceDtoAsync(academicPerformance);

        return Result<GetAcademicPerformanceDto>.Success(mappedDto);
    }

    public async Task<Result<GetAcademicPerformanceDto>> UpdateAsync(UpdateAcademicPerformanceDto dto)
    {
        if (dto == null)
        {
            return Result<GetAcademicPerformanceDto>.Failure("Данные запроса отсутствуют");
        }

        if (dto.Id <= 0)
        {
            return Result<GetAcademicPerformanceDto>.Failure("Некорректный идентификатор успеваемости");
        }

        Result validationResult = ValidateAcademicPerformanceInput(dto.StudentId, dto.DisciplineId, dto.TeacherId, dto.Mark);

        if (validationResult.IsFailure)
        {
            return Result<GetAcademicPerformanceDto>.Failure(validationResult.Error);
        }

        AcademicPerformance? academicPerformance = await _academicPerformanceRepository.GetByIdAsync(dto.Id);

        if (academicPerformance == null)
        {
            return Result<GetAcademicPerformanceDto>.Failure("Запись успеваемости не найдена");
        }

        Student? student = await _studentRepository.GetByIdAsync(dto.StudentId);

        if (student == null)
        {
            return Result<GetAcademicPerformanceDto>.Failure("Указанный студент не найден");
        }

        Discipline? discipline = await _disciplineRepository.GetByIdAsync(dto.DisciplineId);

        if (discipline == null)
        {
            return Result<GetAcademicPerformanceDto>.Failure("Указанная дисциплина не найдена");
        }

        Teacher? teacher = await _teacherRepository.GetByIdAsync(dto.TeacherId);

        if (teacher == null)
        {
            return Result<GetAcademicPerformanceDto>.Failure("Указанный преподаватель не найден");
        }

        if (discipline.TeacherId != dto.TeacherId)
        {
            return Result<GetAcademicPerformanceDto>.Failure("Преподаватель не ведет указанную дисциплину");
        }

        bool duplicateExists = await _academicPerformanceRepository.ExistsAsync(dto.StudentId, dto.DisciplineId, dto.TeacherId, dto.Id);

        if (duplicateExists)
        {
            return Result<GetAcademicPerformanceDto>.Failure("Оценка для указанного студента по этой дисциплине уже существует");
        }

        academicPerformance.StudentId = dto.StudentId;
        academicPerformance.DisciplineId = dto.DisciplineId;
        academicPerformance.TeacherId = dto.TeacherId;
        academicPerformance.Mark = dto.Mark;
        academicPerformance.Student = null!;
        academicPerformance.Discipline = null!;
        academicPerformance.Teacher = null!;

        await _academicPerformanceRepository.UpdateAsync(academicPerformance);

        academicPerformance.Student = student;
        academicPerformance.Discipline = discipline;
        academicPerformance.Teacher = teacher;

        GetAcademicPerformanceDto mappedDto = await MapToGetAcademicPerformanceDtoAsync(academicPerformance);

        return Result<GetAcademicPerformanceDto>.Success(mappedDto);
    }

    public async Task<Result> DeleteAsync(int id)
    {
        if (id <= 0)
        {
            return Result.Failure("Некорректный идентификатор успеваемости");
        }

        AcademicPerformance? academicPerformance = await _academicPerformanceRepository.GetByIdAsync(id);

        if (academicPerformance == null)
        {
            return Result.Failure("Запись успеваемости не найдена");
        }

        await _academicPerformanceRepository.DeleteAsync(academicPerformance);

        return Result.Success();
    }

    private static Result ValidateAcademicPerformanceInput(
        int studentId,
        int disciplineId,
        int teacherId,
        short? mark)
    {
        if (studentId <= 0)
        {
            return Result.Failure("Некорректный идентификатор студента");
        }

        if (disciplineId <= 0)
        {
            return Result.Failure("Некорректный идентификатор дисциплины");
        }

        if (teacherId <= 0)
        {
            return Result.Failure("Некорректный идентификатор преподавателя");
        }

        if (mark == null)
        {
            return Result.Failure("Оценка обязательна");
        }

        if (mark != 2)
        {
            if (mark != 3)
            {
                if (mark != 4)
                {
                    if (mark != 5)
                    {
                        return Result.Failure("Недопустимый формат оценки");
                    }
                }
            }
        }

        return Result.Success();
    }

    private async Task<GetAcademicPerformanceDto> MapToGetAcademicPerformanceDtoAsync(AcademicPerformance academicPerformance)
    {
        Student? student = academicPerformance.Student;
        Discipline? discipline = academicPerformance.Discipline;

        if (student == null)
        {
            student = await _studentRepository.GetByIdAsync(academicPerformance.StudentId);
        }

        if (discipline == null)
        {
            discipline = await _disciplineRepository.GetByIdAsync(academicPerformance.DisciplineId);
        }

        string studentFullName = string.Empty;
        string subjectName = string.Empty;

        if (student != null)
        {
            studentFullName = BuildFullName(student.LastName, student.FirstName, student.FatherName);
        }

        if (discipline != null)
        {
            if (discipline.Subject != null)
            {
                subjectName = discipline.Subject.Name;
            }
        }

        GetAcademicPerformanceDto dto = new GetAcademicPerformanceDto
        {
            Id = academicPerformance.Id,
            StudentFullName = studentFullName,
            SubjectName = subjectName,
            Mark = academicPerformance.Mark
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
