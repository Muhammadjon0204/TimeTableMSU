using Application.Common;
using Application.DTOs.ScheduleDTOs;
using Application.Interfaces.Interface;
using Application.Interfaces.Repository;
using Domain.Entities;
using Domain.Enums;

namespace Application.Service;

public class ScheduleService : IScheduleService
{
    private readonly IScheduleRepository _scheduleRepository;
    private readonly IDisciplineRepository _disciplineRepository;
    private readonly ITeacherRepository _teacherRepository;
    private readonly IAudienceRepository _audienceRepository;
    private readonly IGroupRepository _groupRepository;
    private readonly IWeekRepository _weekRepository;

    public ScheduleService(
        IScheduleRepository scheduleRepository,
        IDisciplineRepository disciplineRepository,
        ITeacherRepository teacherRepository,
        IAudienceRepository audienceRepository,
        IGroupRepository groupRepository,
        IWeekRepository weekRepository)
    {
        _scheduleRepository = scheduleRepository;
        _disciplineRepository = disciplineRepository;
        _teacherRepository = teacherRepository;
        _audienceRepository = audienceRepository;
        _groupRepository = groupRepository;
        _weekRepository = weekRepository;
    }

    public async Task<Result<GetScheduleDto>> GetByIdAsync(int id)
    {
        if (id <= 0)
        {
            return Result<GetScheduleDto>.Failure("Некорректный идентификатор расписания");
        }

        Schedule? schedule = await _scheduleRepository.GetByIdAsync(id);

        if (schedule == null)
        {
            return Result<GetScheduleDto>.Failure("Запись расписания не найдена");
        }

        GetScheduleDto mappedDto = await MapToGetScheduleDtoAsync(schedule);

        return Result<GetScheduleDto>.Success(mappedDto);
    }

    public async Task<Result<PagedResult<GetScheduleDto>>> GetPagedAsync(int pageNumber, int pageSize)
    {
        if (pageNumber < 1)
        {
            pageNumber = 1;
        }

        if (pageSize < 1)
        {
            pageSize = 10;
        }

        PagedResult<Schedule> schedules = await _scheduleRepository.GetPagedAsync(pageNumber, pageSize);

        List<GetScheduleDto> mappedDtos = new List<GetScheduleDto>();

        foreach (Schedule schedule in schedules.Items)
        {
            GetScheduleDto mappedDto = await MapToGetScheduleDtoAsync(schedule);
            mappedDtos.Add(mappedDto);
        }

        PagedResult<GetScheduleDto> result = new PagedResult<GetScheduleDto>(
            mappedDtos,
            schedules.TotalCount,
            schedules.PageNumber,
            schedules.PageSize);

        return Result<PagedResult<GetScheduleDto>>.Success(result);
    }

    public async Task<Result<int>> CreateAsync(CreateScheduleDto dto)
    {
        if (dto == null)
        {
            return Result<int>.Failure("Данные запроса отсутствуют");
        }

        Result<ValidatedScheduleData> validationResult = ValidateScheduleInput(
            dto.Den,
            dto.Para,
            dto.DisciplineId,
            dto.TeacherId,
            dto.AudienceId,
            dto.GroupId,
            dto.WeekId,
            dto.LectureType);

        if (validationResult.IsFailure)
        {
            return Result<int>.Failure(validationResult.Error);
        }

        ValidatedScheduleData validatedData = validationResult.Value;

        Result<ScheduleReferences> referencesResult = await LoadAndValidateReferencesAsync(validatedData);

        if (referencesResult.IsFailure)
        {
            return Result<int>.Failure(referencesResult.Error);
        }

        Result collisionResult = await ValidateScheduleCollisionsAsync(validatedData, null);

        if (collisionResult.IsFailure)
        {
            return Result<int>.Failure(collisionResult.Error);
        }

        ScheduleReferences references = referencesResult.Value;

        Schedule schedule = new Schedule
        {
            Den = validatedData.Den,
            Para = validatedData.Para,
            DisciplineId = validatedData.DisciplineId,
            TeacherId = validatedData.TeacherId,
            AudienceId = validatedData.AudienceId,
            GroupId = validatedData.GroupId,
            WeekId = validatedData.WeekId,
            LectureType = validatedData.LectureType,
            Discipline = references.Discipline,
            Teacher = references.Teacher,
            Audience = references.Audience,
            Group = references.Group,
            Week = references.Week
        };

        await _scheduleRepository.AddAsync(schedule);

        return Result<int>.Success(schedule.Id);
    }

    public async Task<Result<GetScheduleDto>> UpdateAsync(UpdateScheduleDto dto)
    {
        if (dto == null)
        {
            return Result<GetScheduleDto>.Failure("Данные запроса отсутствуют");
        }

        if (dto.Id <= 0)
        {
            return Result<GetScheduleDto>.Failure("Некорректный идентификатор расписания");
        }

        Schedule? schedule = await _scheduleRepository.GetByIdAsync(dto.Id);

        if (schedule == null)
        {
            return Result<GetScheduleDto>.Failure("Запись расписания для обновления не найдена");
        }

        Result<ValidatedScheduleData> validationResult = ValidateScheduleInput(
            dto.Den,
            dto.Para,
            dto.DisciplineId,
            dto.TeacherId,
            dto.AudienceId,
            dto.GroupId,
            dto.WeekId,
            dto.LectureType);

        if (validationResult.IsFailure)
        {
            return Result<GetScheduleDto>.Failure(validationResult.Error);
        }

        ValidatedScheduleData validatedData = validationResult.Value;

        Result<ScheduleReferences> referencesResult = await LoadAndValidateReferencesAsync(validatedData);

        if (referencesResult.IsFailure)
        {
            return Result<GetScheduleDto>.Failure(referencesResult.Error);
        }

        Result collisionResult = await ValidateScheduleCollisionsAsync(validatedData, dto.Id);

        if (collisionResult.IsFailure)
        {
            return Result<GetScheduleDto>.Failure(collisionResult.Error);
        }

        ScheduleReferences references = referencesResult.Value;

        schedule.Den = validatedData.Den;
        schedule.Para = validatedData.Para;
        schedule.DisciplineId = validatedData.DisciplineId;
        schedule.TeacherId = validatedData.TeacherId;
        schedule.AudienceId = validatedData.AudienceId;
        schedule.GroupId = validatedData.GroupId;
        schedule.WeekId = validatedData.WeekId;
        schedule.LectureType = validatedData.LectureType;
        schedule.Discipline = references.Discipline;
        schedule.Teacher = references.Teacher;
        schedule.Audience = references.Audience;
        schedule.Group = references.Group;
        schedule.Week = references.Week;

        await _scheduleRepository.UpdateAsync(schedule);

        GetScheduleDto mappedDto = await MapToGetScheduleDtoAsync(schedule);

        return Result<GetScheduleDto>.Success(mappedDto);
    }

    public async Task<Result> DeleteAsync(int id)
    {
        if (id <= 0)
        {
            return Result.Failure("Некорректный идентификатор расписания");
        }

        Schedule? schedule = await _scheduleRepository.GetByIdAsync(id);

        if (schedule == null)
        {
            return Result.Failure("Запись расписания не найдена");
        }

        await _scheduleRepository.DeleteAsync(schedule);

        return Result.Success();
    }

    private async Task<Result<ScheduleReferences>> LoadAndValidateReferencesAsync(ValidatedScheduleData data)
    {
        Discipline? discipline = await _disciplineRepository.GetByIdAsync(data.DisciplineId);

        if (discipline == null)
        {
            return Result<ScheduleReferences>.Failure("Указанная дисциплина не найдена");
        }

        Teacher? teacher = await _teacherRepository.GetByIdAsync(data.TeacherId);

        if (teacher == null)
        {
            return Result<ScheduleReferences>.Failure("Указанный преподаватель не найден");
        }

        Audience? audience = await _audienceRepository.GetByIdAsync(data.AudienceId);

        if (audience == null)
        {
            return Result<ScheduleReferences>.Failure("Указанная аудитория не найдена");
        }

        Group? group = await _groupRepository.GetByIdAsync(data.GroupId);

        if (group == null)
        {
            return Result<ScheduleReferences>.Failure("Указанная группа не найдена");
        }

        Weeks? week = await _weekRepository.GetByIdAsync(data.WeekId);

        if (week == null)
        {
            return Result<ScheduleReferences>.Failure("Указанная учебная неделя не найдена");
        }

        ScheduleReferences references = new ScheduleReferences
        {
            Discipline = discipline,
            Teacher = teacher,
            Audience = audience,
            Group = group,
            Week = week
        };

        return Result<ScheduleReferences>.Success(references);
    }

    private async Task<Result> ValidateScheduleCollisionsAsync(ValidatedScheduleData data, int? excludedScheduleId)
    {
        List<Schedule> schedules = await _scheduleRepository.GetAllAsync();

        foreach (Schedule existingSchedule in schedules)
        {
            if (excludedScheduleId.HasValue)
            {
                if (existingSchedule.Id == excludedScheduleId.Value)
                {
                    continue;
                }
            }

            if (existingSchedule.Den == data.Den)
            {
                if (existingSchedule.Para == data.Para)
                {
                    if (existingSchedule.WeekId == data.WeekId)
                    {
                        if (existingSchedule.AudienceId == data.AudienceId)
                        {
                            return Result.Failure("Указанная аудитория уже занята на этой паре");
                        }

                        if (existingSchedule.GroupId == data.GroupId)
                        {
                            return Result.Failure("У данной группы уже есть занятие на этой паре");
                        }

                        if (existingSchedule.TeacherId == data.TeacherId)
                        {
                            return Result.Failure("Преподаватель уже ведет другое занятие на этой паре");
                        }
                    }
                }
            }
        }

        return Result.Success();
    }

    private static Result<ValidatedScheduleData> ValidateScheduleInput(
        int den,
        int para,
        int disciplineId,
        int teacherId,
        int audienceId,
        int groupId,
        int weekId,
        string lectureType)
    {
        if (den < 1)
        {
            return Result<ValidatedScheduleData>.Failure("Некорректный день недели");
        }

        if (den > 6)
        {
            return Result<ValidatedScheduleData>.Failure("Некорректный день недели");
        }

        if (para < 1)
        {
            return Result<ValidatedScheduleData>.Failure("Некорректный номер пары");
        }

        if (para > 7)
        {
            return Result<ValidatedScheduleData>.Failure("Некорректный номер пары");
        }

        if (disciplineId <= 0)
        {
            return Result<ValidatedScheduleData>.Failure("Некорректный идентификатор дисциплины");
        }

        if (teacherId <= 0)
        {
            return Result<ValidatedScheduleData>.Failure("Некорректный идентификатор преподавателя");
        }

        if (audienceId <= 0)
        {
            return Result<ValidatedScheduleData>.Failure("Некорректный идентификатор аудитории");
        }

        if (groupId <= 0)
        {
            return Result<ValidatedScheduleData>.Failure("Некорректный идентификатор группы");
        }

        if (weekId <= 0)
        {
            return Result<ValidatedScheduleData>.Failure("Некорректный идентификатор учебной недели");
        }

        if (string.IsNullOrWhiteSpace(lectureType))
        {
            return Result<ValidatedScheduleData>.Failure("Тип занятия не может быть пустым");
        }

        string trimmedLectureType = lectureType.Trim();

        if (ContainsDangerousCharacters(trimmedLectureType))
        {
            return Result<ValidatedScheduleData>.Failure("Обнаружены недопустимые символы в типе занятия");
        }

        if (trimmedLectureType != "Lecture")
        {
            if (trimmedLectureType != "Practice")
            {
                if (trimmedLectureType != "Laboratory")
                {
                    return Result<ValidatedScheduleData>.Failure("Некорректный тип занятия");
                }
            }
        }

        LectureType parsedLectureType;

        if (trimmedLectureType == "Lecture")
        {
            parsedLectureType = LectureType.Lecture;
        }
        else if (trimmedLectureType == "Practice")
        {
            parsedLectureType = LectureType.Practice;
        }
        else
        {
            parsedLectureType = LectureType.Laboratory;
        }

        ValidatedScheduleData data = new ValidatedScheduleData
        {
            Den = (short)den,
            Para = (short)para,
            DisciplineId = disciplineId,
            TeacherId = teacherId,
            AudienceId = audienceId,
            GroupId = groupId,
            WeekId = weekId,
            LectureType = parsedLectureType
        };

        return Result<ValidatedScheduleData>.Success(data);
    }

    private async Task<GetScheduleDto> MapToGetScheduleDtoAsync(Schedule schedule)
    {
        Discipline? discipline = schedule.Discipline;
        Teacher? teacher = schedule.Teacher;
        Audience? audience = schedule.Audience;
        Group? group = schedule.Group;

        if (discipline == null)
        {
            discipline = await _disciplineRepository.GetByIdAsync(schedule.DisciplineId);
        }

        if (teacher == null)
        {
            teacher = await _teacherRepository.GetByIdAsync(schedule.TeacherId);
        }

        if (audience == null)
        {
            audience = await _audienceRepository.GetByIdAsync(schedule.AudienceId);
        }

        if (group == null)
        {
            group = await _groupRepository.GetByIdAsync(schedule.GroupId);
        }

        string subjectName = string.Empty;
        int subjectId = 0;

        if (discipline != null)
        {
            subjectId = discipline.SubjectId;

            if (discipline.Subject != null)
            {
                if (!string.IsNullOrWhiteSpace(discipline.Subject.Name))
                {
                    subjectName = discipline.Subject.Name;
                }
            }
        }

        string teacherFullName = string.Empty;

        if (teacher != null)
        {
            teacherFullName = BuildFullName(teacher.LastName, teacher.FirstName, teacher.FatherName);
        }

        string audienceNumber = string.Empty;

        if (audience != null)
        {
            if (!string.IsNullOrWhiteSpace(audience.Number))
            {
                audienceNumber = audience.Number;
            }
        }

        string groupName = string.Empty;

        if (group != null)
        {
            if (!string.IsNullOrWhiteSpace(group.Name))
            {
                groupName = group.Name;
            }
        }

        string lectureType = string.Empty;

        if (schedule.LectureType.HasValue)
        {
            lectureType = schedule.LectureType.Value.ToString();
        }

        GetScheduleDto dto = new GetScheduleDto
        {
            Id = schedule.Id,
            Den = schedule.Den,
            Para = schedule.Para,
            SubjectId = subjectId,
            DisciplineId = schedule.DisciplineId,
            TeacherId = schedule.TeacherId,
            AudienceId = schedule.AudienceId,
            GroupId = schedule.GroupId,
            WeekId = schedule.WeekId,
            SubjectName = subjectName,
            TeacherFullName = teacherFullName,
            AudienceNumber = audienceNumber,
            GroupName = groupName,
            LectureType = lectureType
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

    private static bool ContainsDangerousCharacters(string value)
    {
        if (value.Contains('<'))
        {
            return true;
        }

        if (value.Contains('>'))
        {
            return true;
        }

        if (value.Contains(';'))
        {
            return true;
        }

        if (value.IndexOf("<script", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return true;
        }

        if (value.IndexOf("</script", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return true;
        }

        if (value.IndexOf("script>", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return true;
        }

        if (value.IndexOf("javascript:", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return true;
        }

        if (value.IndexOf("onerror=", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return true;
        }

        if (value.IndexOf("onload=", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return true;
        }

        return false;
    }

    private class ValidatedScheduleData
    {
        public short Den { get; set; }
        public short Para { get; set; }
        public int DisciplineId { get; set; }
        public int TeacherId { get; set; }
        public int AudienceId { get; set; }
        public int GroupId { get; set; }
        public int WeekId { get; set; }
        public LectureType LectureType { get; set; }
    }

    private class ScheduleReferences
    {
        public Discipline Discipline { get; set; } = null!;
        public Teacher Teacher { get; set; } = null!;
        public Audience Audience { get; set; } = null!;
        public Group Group { get; set; } = null!;
        public Weeks Week { get; set; } = null!;
    }
}
