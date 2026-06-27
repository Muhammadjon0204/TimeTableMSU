using Application.Common;
using Application.DTOs.ExecutionDTOs;
using Application.Interfaces.Interface;
using Application.Interfaces.Repository;
using Domain.Entities;

namespace Application.Service;

public class ExecutionService : IExecutionService
{
    private readonly IExecutionRepository _executionRepository;
    private readonly IScheduleRepository _scheduleRepository;

    public ExecutionService(
        IExecutionRepository executionRepository,
        IScheduleRepository scheduleRepository)
    {
        _executionRepository = executionRepository;
        _scheduleRepository = scheduleRepository;
    }

    public async Task<Result<GetExecutionDto>> GetByIdAsync(int id)
    {
        if (id <= 0)
        {
            return Result<GetExecutionDto>.Failure("Некорректный идентификатор записи");
        }

        Execution? execution = await _executionRepository.GetByIdAsync(id);

        if (execution == null)
        {
            return Result<GetExecutionDto>.Failure("Запись не найдена");
        }

        GetExecutionDto mappedDto = await MapToGetExecutionDtoAsync(execution);

        return Result<GetExecutionDto>.Success(mappedDto);
    }

    public async Task<Result<List<GetExecutionDto>>> GetAllAsync()
    {
        List<Execution> executions = await _executionRepository.GetAllAsync();

        List<GetExecutionDto> mappedDtos = new List<GetExecutionDto>();

        foreach (Execution execution in executions)
        {
            GetExecutionDto mappedDto = await MapToGetExecutionDtoAsync(execution);
            mappedDtos.Add(mappedDto);
        }

        return Result<List<GetExecutionDto>>.Success(mappedDtos);
    }

    public async Task<Result<int>> CreateAsync(CreateExecutionDto dto)
    {
        if (dto == null)
        {
            return Result<int>.Failure("Данные запроса отсутствуют");
        }

        Result<ValidatedExecutionData> validationResult = ValidateExecutionData(
            dto.ScheduleId,
            dto.ExecutionDate,
            dto.Status,
            dto.Notes);

        if (validationResult.IsFailure)
        {
            return Result<int>.Failure(validationResult.Error);
        }

        ValidatedExecutionData validatedData = validationResult.Value;

        Schedule? schedule = await _scheduleRepository.GetByIdAsync(validatedData.ScheduleId);

        if (schedule == null)
        {
            return Result<int>.Failure("Указанное занятие в расписании не найдено");
        }

        Execution execution = new Execution
        {
            ScheduleId = validatedData.ScheduleId,
            ExecutionDate = validatedData.ExecutionDate,
            Status = validatedData.Status,
            Notes = validatedData.Notes,
            Schedule = schedule
        };

        await _executionRepository.AddAsync(execution);

        return Result<int>.Success(execution.Id);
    }

    public async Task<Result<GetExecutionDto>> UpdateAsync(UpdateExecutionDto dto)
    {
        if (dto == null)
        {
            return Result<GetExecutionDto>.Failure("Данные запроса отсутствуют");
        }

        if (dto.Id <= 0)
        {
            return Result<GetExecutionDto>.Failure("Некорректный идентификатор записи");
        }

        Result<ValidatedExecutionData> validationResult = ValidateExecutionData(
            dto.ScheduleId,
            dto.ExecutionDate,
            dto.Status,
            dto.Notes);

        if (validationResult.IsFailure)
        {
            return Result<GetExecutionDto>.Failure(validationResult.Error);
        }

        ValidatedExecutionData validatedData = validationResult.Value;

        Execution? execution = await _executionRepository.GetByIdAsync(dto.Id);

        if (execution == null)
        {
            return Result<GetExecutionDto>.Failure("Запись о выполнении не найдена");
        }

        Schedule? schedule = await _scheduleRepository.GetByIdAsync(validatedData.ScheduleId);

        if (schedule == null)
        {
            return Result<GetExecutionDto>.Failure("Указанное занятие в расписании не найдено");
        }

        execution.ScheduleId = validatedData.ScheduleId;
        execution.ExecutionDate = validatedData.ExecutionDate;
        execution.Status = validatedData.Status;
        execution.Notes = validatedData.Notes;
        execution.Schedule = schedule;

        await _executionRepository.UpdateAsync(execution);

        GetExecutionDto mappedDto = await MapToGetExecutionDtoAsync(execution);

        return Result<GetExecutionDto>.Success(mappedDto);
    }

    public async Task<Result> DeleteAsync(int id)
    {
        if (id <= 0)
        {
            return Result.Failure("Некорректный идентификатор записи");
        }

        Execution? execution = await _executionRepository.GetByIdAsync(id);

        if (execution == null)
        {
            return Result.Failure("Запись о выполнении не найдена");
        }

        await _executionRepository.DeleteAsync(execution);

        return Result.Success();
    }

    private static Result<ValidatedExecutionData> ValidateExecutionData(
        int scheduleId,
        DateOnly executionDate,
        string status,
        string? notes)
    {
        if (scheduleId <= 0)
        {
            return Result<ValidatedExecutionData>.Failure("Некорректный идентификатор занятия в расписании");
        }

        if (executionDate == default)
        {
            return Result<ValidatedExecutionData>.Failure("Дата выполнения не указана");
        }

        DateOnly today = DateOnly.FromDateTime(DateTime.Today);
        DateOnly minimumAllowedDate = today.AddYears(-1);

        if (executionDate < minimumAllowedDate)
        {
            return Result<ValidatedExecutionData>.Failure("Дата выполнения не может быть более чем год назад");
        }

        if (string.IsNullOrWhiteSpace(status))
        {
            return Result<ValidatedExecutionData>.Failure("Статус выполнения не может быть пустым");
        }

        string trimmedStatus = status.Trim();

        if (ContainsDangerousCharacters(trimmedStatus))
        {
            return Result<ValidatedExecutionData>.Failure("Обнаружены недопустимые символы в статусе выполнения");
        }

        if (trimmedStatus != "Conducted")
        {
            if (trimmedStatus != "Canceled")
            {
                if (trimmedStatus != "Moved")
                {
                    return Result<ValidatedExecutionData>.Failure("Указан недопустимый статус выполнения");
                }
            }
        }

        string? normalizedNotes = null;

        if (!string.IsNullOrWhiteSpace(notes))
        {
            string trimmedNotes = notes.Trim();

            if (trimmedNotes.Length > 500)
            {
                return Result<ValidatedExecutionData>.Failure("Длина примечания не должна превышать 500 символов");
            }

            if (ContainsDangerousCharacters(trimmedNotes))
            {
                return Result<ValidatedExecutionData>.Failure("Обнаружены недопустимые символы в примечании");
            }

            normalizedNotes = trimmedNotes;
        }

        ValidatedExecutionData data = new ValidatedExecutionData
        {
            ScheduleId = scheduleId,
            ExecutionDate = executionDate,
            Status = trimmedStatus,
            Notes = normalizedNotes
        };

        return Result<ValidatedExecutionData>.Success(data);
    }

    private async Task<GetExecutionDto> MapToGetExecutionDtoAsync(Execution execution)
    {
        Schedule? schedule = execution.Schedule;

        if (schedule == null)
        {
            schedule = await _scheduleRepository.GetByIdAsync(execution.ScheduleId);
        }

        string scheduleInfo = string.Empty;

        if (schedule != null)
        {
            scheduleInfo = BuildScheduleInfo(schedule);
        }

        GetExecutionDto dto = new GetExecutionDto
        {
            Id = execution.Id,
            ScheduleId = execution.ScheduleId,
            ScheduleInfo = scheduleInfo,
            ExecutionDate = execution.ExecutionDate,
            Status = execution.Status,
            Notes = execution.Notes
        };

        return dto;
    }

    private static string BuildScheduleInfo(Schedule schedule)
    {
        string subjectName = string.Empty;
        string groupName = string.Empty;

        Discipline? discipline = schedule.Discipline;

        if (discipline != null)
        {
            Subject? subject = discipline.Subject;

            if (subject != null)
            {
                if (!string.IsNullOrWhiteSpace(subject.Name))
                {
                    subjectName = subject.Name;
                }
            }
        }

        Group? group = schedule.Group;

        if (group != null)
        {
            if (!string.IsNullOrWhiteSpace(group.Name))
            {
                groupName = group.Name;
            }
        }

        if (!string.IsNullOrWhiteSpace(subjectName))
        {
            if (!string.IsNullOrWhiteSpace(groupName))
            {
                return $"{subjectName} - {groupName}";
            }
        }

        if (!string.IsNullOrWhiteSpace(subjectName))
        {
            return subjectName;
        }

        if (!string.IsNullOrWhiteSpace(groupName))
        {
            return groupName;
        }

        return $"Расписание #{schedule.Id}";
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

    private class ValidatedExecutionData
    {
        public int ScheduleId { get; set; }
        public DateOnly ExecutionDate { get; set; }
        public string Status { get; set; } = null!;
        public string? Notes { get; set; }
    }
}
