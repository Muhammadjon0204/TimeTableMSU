using Application.Common;
using Application.DTOs.DisciplineDTOs;
using Application.Interfaces.Interface;
using Application.Interfaces.Repository;
using Domain.Entities;

namespace Application.Service;

public class DisciplineService : IDisciplineService
{
    private readonly IDisciplineRepository _disciplineRepository;
    private readonly ISubjectRepository _subjectRepository;
    private readonly ITeacherRepository _teacherRepository;
    private readonly IGroupRepository _groupRepository;

    public DisciplineService(
        IDisciplineRepository disciplineRepository,
        ISubjectRepository subjectRepository,
        ITeacherRepository teacherRepository,
        IGroupRepository groupRepository)
    {
        _disciplineRepository = disciplineRepository;
        _subjectRepository = subjectRepository;
        _teacherRepository = teacherRepository;
        _groupRepository = groupRepository;
    }

    public async Task<Result<GetDisciplineDto>> GetByIdAsync(int id)
    {
        if (id <= 0)
        {
            return Result<GetDisciplineDto>.Failure("Некорректный идентификатор дисциплины");
        }

        Discipline? discipline = await _disciplineRepository.GetByIdAsync(id);

        if (discipline == null)
        {
            return Result<GetDisciplineDto>.Failure("Дисциплина не найдена");
        }

        GetDisciplineDto mappedDto = await MapToGetDisciplineDtoAsync(discipline);

        return Result<GetDisciplineDto>.Success(mappedDto);
    }

    public async Task<Result<List<GetDisciplineDto>>> GetAllAsync()
    {
        List<Discipline> disciplines = await _disciplineRepository.GetAllAsync();

        List<GetDisciplineDto> mappedDtos = new List<GetDisciplineDto>();

        foreach (Discipline discipline in disciplines)
        {
            GetDisciplineDto mappedDto = await MapToGetDisciplineDtoAsync(discipline);
            mappedDtos.Add(mappedDto);
        }

        return Result<List<GetDisciplineDto>>.Success(mappedDtos);
    }

    public async Task<Result<GetDisciplineDto>> CreateAsync(CreateDisciplineDto dto)
    {
        if (dto == null)
        {
            return Result<GetDisciplineDto>.Failure("Данные запроса отсутствуют");
        }

        Result validationResult = ValidateDisciplineInput(
            dto.SubjectId,
            dto.TeacherId,
            dto.GroupId,
            dto.LectureHourCount,
            dto.PracticeHourCount,
            dto.LaboratoryHourCount);

        if (validationResult.IsFailure)
        {
            return Result<GetDisciplineDto>.Failure(validationResult.Error);
        }

        Subject? subject = await _subjectRepository.GetByIdAsync(dto.SubjectId);

        if (subject == null)
        {
            return Result<GetDisciplineDto>.Failure("Указанный предмет не найден");
        }

        Teacher? teacher = await _teacherRepository.GetByIdAsync(dto.TeacherId);

        if (teacher == null)
        {
            return Result<GetDisciplineDto>.Failure("Указанный преподаватель не найден");
        }

        Group? group = await _groupRepository.GetByIdAsync(dto.GroupId);

        if (group == null)
        {
            return Result<GetDisciplineDto>.Failure("Указанная группа не найдена");
        }

        List<Discipline> disciplines = await _disciplineRepository.GetAllAsync();

        foreach (Discipline existingDiscipline in disciplines)
        {
            if (existingDiscipline.SubjectId == dto.SubjectId)
            {
                if (existingDiscipline.TeacherId == dto.TeacherId)
                {
                    if (existingDiscipline.GroupId == dto.GroupId)
                    {
                        return Result<GetDisciplineDto>.Failure("Такая дисциплина уже существует для указанной группы, предмета и преподавателя");
                    }
                }
            }
        }

        Discipline discipline = new Discipline
        {
            SubjectId = dto.SubjectId,
            TeacherId = dto.TeacherId,
            GroupId = dto.GroupId,
            LectureHourCount = dto.LectureHourCount,
            PracticeHourCount = dto.PracticeHourCount,
            LaboratoryHourCount = dto.LaboratoryHourCount
        };

        await _disciplineRepository.AddAsync(discipline);

        GetDisciplineDto mappedDto = await MapToGetDisciplineDtoAsync(discipline);

        return Result<GetDisciplineDto>.Success(mappedDto);
    }

    public async Task<Result<GetDisciplineDto>> UpdateAsync(UpdateDisciplineDto dto)
    {
        if (dto == null)
        {
            return Result<GetDisciplineDto>.Failure("Данные запроса отсутствуют");
        }

        if (dto.Id <= 0)
        {
            return Result<GetDisciplineDto>.Failure("Некорректный идентификатор дисциплины");
        }

        Result validationResult = ValidateDisciplineInput(
            dto.SubjectId,
            dto.TeacherId,
            dto.GroupId,
            dto.LectureHourCount,
            dto.PracticeHourCount,
            dto.LaboratoryHourCount);

        if (validationResult.IsFailure)
        {
            return Result<GetDisciplineDto>.Failure(validationResult.Error);
        }

        Discipline? discipline = await _disciplineRepository.GetByIdAsync(dto.Id);

        if (discipline == null)
        {
            return Result<GetDisciplineDto>.Failure("Дисциплина не найдена");
        }

        Subject? subject = await _subjectRepository.GetByIdAsync(dto.SubjectId);

        if (subject == null)
        {
            return Result<GetDisciplineDto>.Failure("Указанный предмет не найден");
        }

        Teacher? teacher = await _teacherRepository.GetByIdAsync(dto.TeacherId);

        if (teacher == null)
        {
            return Result<GetDisciplineDto>.Failure("Указанный преподаватель не найден");
        }

        Group? group = await _groupRepository.GetByIdAsync(dto.GroupId);

        if (group == null)
        {
            return Result<GetDisciplineDto>.Failure("Указанная группа не найдена");
        }

        List<Discipline> disciplines = await _disciplineRepository.GetAllAsync();

        foreach (Discipline existingDiscipline in disciplines)
        {
            if (existingDiscipline.Id != dto.Id)
            {
                if (existingDiscipline.SubjectId == dto.SubjectId)
                {
                    if (existingDiscipline.TeacherId == dto.TeacherId)
                    {
                        if (existingDiscipline.GroupId == dto.GroupId)
                        {
                            return Result<GetDisciplineDto>.Failure("Такая дисциплина уже существует для указанной группы, предмета и преподавателя");
                        }
                    }
                }
            }
        }

        discipline.SubjectId = dto.SubjectId;
        discipline.TeacherId = dto.TeacherId;
        discipline.GroupId = dto.GroupId;
        discipline.LectureHourCount = dto.LectureHourCount;
        discipline.PracticeHourCount = dto.PracticeHourCount;
        discipline.LaboratoryHourCount = dto.LaboratoryHourCount;

        await _disciplineRepository.UpdateAsync(discipline);

        GetDisciplineDto mappedDto = await MapToGetDisciplineDtoAsync(discipline);

        return Result<GetDisciplineDto>.Success(mappedDto);
    }

    public async Task<Result> DeleteAsync(int id)
    {
        if (id <= 0)
        {
            return Result.Failure("Некорректный идентификатор дисциплины");
        }

        Discipline? discipline = await _disciplineRepository.GetByIdAsync(id);

        if (discipline == null)
        {
            return Result.Failure("Дисциплина не найдена");
        }

        await _disciplineRepository.DeleteAsync(discipline);

        return Result.Success();
    }

    private static Result ValidateDisciplineInput(
        int subjectId,
        int teacherId,
        int groupId,
        int? lectureHourCount,
        int? practiceHourCount,
        int? laboratoryHourCount)
    {
        if (subjectId <= 0)
        {
            return Result.Failure("Некорректный идентификатор предмета");
        }

        if (teacherId <= 0)
        {
            return Result.Failure("Некорректный идентификатор преподавателя");
        }

        if (groupId <= 0)
        {
            return Result.Failure("Некорректный идентификатор группы");
        }

        if (lectureHourCount.HasValue)
        {
            if (lectureHourCount.Value < 0)
            {
                return Result.Failure("Количество лекционных часов не может быть меньше 0");
            }
        }

        if (practiceHourCount.HasValue)
        {
            if (practiceHourCount.Value < 0)
            {
                return Result.Failure("Количество практических часов не может быть меньше 0");
            }
        }

        if (laboratoryHourCount.HasValue)
        {
            if (laboratoryHourCount.Value < 0)
            {
                return Result.Failure("Количество лабораторных часов не может быть меньше 0");
            }
        }

        int lectureHours = lectureHourCount ?? 0;
        int practiceHours = practiceHourCount ?? 0;
        int laboratoryHours = laboratoryHourCount ?? 0;
        int totalHours = lectureHours + practiceHours + laboratoryHours;

        if (totalHours <= 0)
        {
            return Result.Failure("Общая сумма часов должна быть больше 0");
        }

        return Result.Success();
    }

    private async Task<GetDisciplineDto> MapToGetDisciplineDtoAsync(Discipline discipline)
    {
        string? subjectName = discipline.Subject?.Name;
        string? teacherFullName = null;
        string? groupName = discipline.Group?.Name;

        if (string.IsNullOrWhiteSpace(subjectName))
        {
            Subject? subject = await _subjectRepository.GetByIdAsync(discipline.SubjectId);

            if (subject != null)
            {
                subjectName = subject.Name;
            }
        }

        Teacher? teacher = discipline.Teacher;

        if (teacher == null)
        {
            teacher = await _teacherRepository.GetByIdAsync(discipline.TeacherId);
        }

        if (teacher != null)
        {
            teacherFullName = BuildFullName(teacher.LastName, teacher.FirstName, teacher.FatherName);
        }

        if (string.IsNullOrWhiteSpace(groupName))
        {
            Group? group = await _groupRepository.GetByIdAsync(discipline.GroupId);

            if (group != null)
            {
                groupName = group.Name;
            }
        }

        GetDisciplineDto dto = new GetDisciplineDto
        {
            Id = discipline.Id,
            SubjectId = discipline.SubjectId,
            SubjectName = subjectName ?? string.Empty,
            TeacherId = discipline.TeacherId,
            TeacherFullName = teacherFullName ?? string.Empty,
            GroupId = discipline.GroupId,
            GroupName = groupName ?? string.Empty,
            LectureHourCount = discipline.LectureHourCount,
            PracticeHourCount = discipline.PracticeHourCount,
            LaboratoryHourCount = discipline.LaboratoryHourCount
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
