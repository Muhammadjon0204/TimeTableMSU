using Application.Common;
using Application.DTOs.WeekDTOs;
using Application.Interfaces.Interface;
using Application.Interfaces.Repository;
using Domain.Entities;

namespace Application.Service;

public class WeekService : IWeekService
{
    private readonly IWeekRepository _weekRepository;

    public WeekService(IWeekRepository weekRepository)
    {
        _weekRepository = weekRepository;
    }

    public async Task<Result<GetWeekDto>> GetByIdAsync(int id)
    {
        if (id <= 0)
        {
            return Result<GetWeekDto>.Failure("Некорректный идентификатор недели");
        }

        Weeks? week = await _weekRepository.GetByIdAsync(id);

        if (week == null)
        {
            return Result<GetWeekDto>.Failure("Учебная неделя не найдена");
        }

        return Result<GetWeekDto>.Success(MapToGetWeekDto(week));
    }

    public async Task<Result<List<GetWeekDto>>> GetAllAsync()
    {
        List<Weeks> weeks = await _weekRepository.GetAllAsync();
        return Result<List<GetWeekDto>>.Success(weeks.Select(MapToGetWeekDto).ToList());
    }

    public async Task<Result<GetWeekDto>> CreateAsync(CreateWeekDto dto)
    {
        if (dto == null)
        {
            return Result<GetWeekDto>.Failure("Данные запроса отсутствуют");
        }

        Result<ValidatedWeekData> validationResult = ValidateWeekData(dto.Name, dto.StartDate, dto.EndDate, dto.AcademicYearId);

        if (validationResult.IsFailure)
        {
            return Result<GetWeekDto>.Failure(validationResult.Error);
        }

        ValidatedWeekData data = validationResult.Value;
        Result duplicateResult = await EnsureNoDuplicatesAsync(data, null);

        if (duplicateResult.IsFailure)
        {
            return Result<GetWeekDto>.Failure(duplicateResult.Error);
        }

        Weeks week = new()
        {
            Name = data.Name,
            StartDate = data.StartDate.ToDateTime(TimeOnly.MinValue),
            EndDate = data.EndDate.ToDateTime(TimeOnly.MinValue),
            AcademicYearId = data.AcademicYearId
        };

        await _weekRepository.AddAsync(week);

        return Result<GetWeekDto>.Success(MapToGetWeekDto(week));
    }

    public async Task<Result<GetWeekDto>> UpdateAsync(UpdateWeekDto dto)
    {
        if (dto == null)
        {
            return Result<GetWeekDto>.Failure("Данные запроса отсутствуют");
        }

        if (dto.Id <= 0)
        {
            return Result<GetWeekDto>.Failure("Некорректный идентификатор недели");
        }

        Result<ValidatedWeekData> validationResult = ValidateWeekData(dto.Name, dto.StartDate, dto.EndDate, dto.AcademicYearId);

        if (validationResult.IsFailure)
        {
            return Result<GetWeekDto>.Failure(validationResult.Error);
        }

        Weeks? week = await _weekRepository.GetByIdAsync(dto.Id);

        if (week == null)
        {
            return Result<GetWeekDto>.Failure("Учебная неделя для обновления не найдена");
        }

        ValidatedWeekData data = validationResult.Value;
        Result duplicateResult = await EnsureNoDuplicatesAsync(data, dto.Id);

        if (duplicateResult.IsFailure)
        {
            return Result<GetWeekDto>.Failure(duplicateResult.Error);
        }

        week.Name = data.Name;
        week.StartDate = data.StartDate.ToDateTime(TimeOnly.MinValue);
        week.EndDate = data.EndDate.ToDateTime(TimeOnly.MinValue);
        week.AcademicYearId = data.AcademicYearId;

        await _weekRepository.UpdateAsync(week);

        return Result<GetWeekDto>.Success(MapToGetWeekDto(week));
    }

    public async Task<Result> DeleteAsync(int id)
    {
        if (id <= 0)
        {
            return Result.Failure("Некорректный идентификатор недели");
        }

        Weeks? week = await _weekRepository.GetByIdAsync(id);

        if (week == null)
        {
            return Result.Failure("Учебная неделя не найдена");
        }

        await _weekRepository.DeleteAsync(week);

        return Result.Success();
    }

    private async Task<Result> EnsureNoDuplicatesAsync(ValidatedWeekData data, int? excludedId)
    {
        List<Weeks> weeks = await _weekRepository.GetByAcademicYearAsync(data.AcademicYearId);

        foreach (Weeks existingWeek in weeks.Where(week => week.Id != excludedId))
        {
            if (string.Equals(existingWeek.Name?.Trim(), data.Name, StringComparison.OrdinalIgnoreCase))
            {
                return Result.Failure("Неделя с таким названием уже существует в выбранном учебном году");
            }

            if (HasDateIntersection(existingWeek, data.StartDate, data.EndDate))
            {
                return Result.Failure("Указанный период пересекается с существующей неделей выбранного учебного года");
            }
        }

        return Result.Success();
    }

    private static Result<ValidatedWeekData> ValidateWeekData(string name, DateOnly startDate, DateOnly endDate, int? academicYearId)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result<ValidatedWeekData>.Failure("Название недели не может быть пустым");
        }

        string trimmedName = name.Trim();

        if (trimmedName.Length < 2 || trimmedName.Length > 50)
        {
            return Result<ValidatedWeekData>.Failure("Длина названия недели должна быть от 2 до 50 символов");
        }

        if (ContainsDangerousCharacters(trimmedName))
        {
            return Result<ValidatedWeekData>.Failure("Обнаружены недопустимые символы в названии недели");
        }

        if (!academicYearId.HasValue || academicYearId.Value <= 0)
        {
            return Result<ValidatedWeekData>.Failure("Выберите учебный год");
        }

        if (startDate == default)
        {
            return Result<ValidatedWeekData>.Failure("Дата начала недели не указана");
        }

        if (endDate == default)
        {
            return Result<ValidatedWeekData>.Failure("Дата окончания недели не указана");
        }

        if (startDate >= endDate)
        {
            return Result<ValidatedWeekData>.Failure("Дата начала недели должна быть раньше даты окончания");
        }

        return Result<ValidatedWeekData>.Success(new ValidatedWeekData
        {
            Name = trimmedName,
            StartDate = startDate,
            EndDate = endDate,
            AcademicYearId = academicYearId.Value
        });
    }

    private static GetWeekDto MapToGetWeekDto(Weeks week)
    {
        return new GetWeekDto
        {
            Id = week.Id,
            Name = week.Name ?? string.Empty,
            StartDate = DateOnly.FromDateTime(week.StartDate),
            EndDate = DateOnly.FromDateTime(week.EndDate),
            AcademicYearId = week.AcademicYearId,
            AcademicYearName = week.AcademicYear?.Name ?? string.Empty,
            WeekType = week.Type.ToString(),
            IsCurrent = week.IsCurrent
        };
    }

    private static bool HasDateIntersection(Weeks existingWeek, DateOnly startDate, DateOnly endDate)
    {
        DateOnly existingStartDate = DateOnly.FromDateTime(existingWeek.StartDate);
        DateOnly existingEndDate = DateOnly.FromDateTime(existingWeek.EndDate);

        return startDate < existingEndDate && endDate > existingStartDate;
    }

    private static bool ContainsDangerousCharacters(string value)
    {
        return value.Contains('<')
            || value.Contains('>')
            || value.Contains(';')
            || value.IndexOf("<script", StringComparison.OrdinalIgnoreCase) >= 0
            || value.IndexOf("</script", StringComparison.OrdinalIgnoreCase) >= 0
            || value.IndexOf("script>", StringComparison.OrdinalIgnoreCase) >= 0
            || value.IndexOf("javascript:", StringComparison.OrdinalIgnoreCase) >= 0
            || value.IndexOf("onerror=", StringComparison.OrdinalIgnoreCase) >= 0
            || value.IndexOf("onload=", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private class ValidatedWeekData
    {
        public string Name { get; set; } = string.Empty;
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public int AcademicYearId { get; set; }
    }
}
