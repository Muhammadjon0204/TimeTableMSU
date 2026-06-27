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

        GetWeekDto mappedDto = MapToGetWeekDto(week);

        return Result<GetWeekDto>.Success(mappedDto);
    }

    public async Task<Result<List<GetWeekDto>>> GetAllAsync()
    {
        List<Weeks> weeks = await _weekRepository.GetAllAsync();

        List<GetWeekDto> mappedDtos = new List<GetWeekDto>();

        foreach (Weeks week in weeks)
        {
            GetWeekDto mappedDto = MapToGetWeekDto(week);
            mappedDtos.Add(mappedDto);
        }

        return Result<List<GetWeekDto>>.Success(mappedDtos);
    }

    public async Task<Result<GetWeekDto>> CreateAsync(CreateWeekDto dto)
    {
        if (dto == null)
        {
            return Result<GetWeekDto>.Failure("Данные запроса отсутствуют");
        }

        Result<ValidatedWeekData> validationResult = ValidateWeekData(
            dto.Name,
            dto.StartDate,
            dto.EndDate);

        if (validationResult.IsFailure)
        {
            return Result<GetWeekDto>.Failure(validationResult.Error);
        }

        ValidatedWeekData validatedData = validationResult.Value;

        List<Weeks> weeks = await _weekRepository.GetAllAsync();

        foreach (Weeks existingWeek in weeks)
        {
            if (existingWeek.Name != null)
            {
                if (string.Equals(existingWeek.Name.Trim(), validatedData.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return Result<GetWeekDto>.Failure("Учебная неделя с таким названием уже существует");
                }
            }
        }

        foreach (Weeks existingWeek in weeks)
        {
            if (HasDateIntersection(existingWeek, validatedData.StartDate, validatedData.EndDate))
            {
                return Result<GetWeekDto>.Failure("Указанный временной период пересекается с уже существующей неделей");
            }
        }

        Weeks week = new Weeks
        {
            Name = validatedData.Name,
            StartDate = validatedData.StartDate.ToDateTime(TimeOnly.MinValue),
            EndDate = validatedData.EndDate.ToDateTime(TimeOnly.MinValue)
        };

        await _weekRepository.AddAsync(week);

        GetWeekDto mappedDto = MapToGetWeekDto(week);

        return Result<GetWeekDto>.Success(mappedDto);
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

        Result<ValidatedWeekData> validationResult = ValidateWeekData(
            dto.Name,
            dto.StartDate,
            dto.EndDate);

        if (validationResult.IsFailure)
        {
            return Result<GetWeekDto>.Failure(validationResult.Error);
        }

        ValidatedWeekData validatedData = validationResult.Value;

        Weeks? week = await _weekRepository.GetByIdAsync(dto.Id);

        if (week == null)
        {
            return Result<GetWeekDto>.Failure("Учебная неделя для обновления не найдена");
        }

        DateOnly currentStartDate = ConvertToDateOnly(week.StartDate);
        DateOnly currentEndDate = ConvertToDateOnly(week.EndDate);

        bool nameChanged = true;

        if (week.Name != null)
        {
            nameChanged = !string.Equals(week.Name.Trim(), validatedData.Name, StringComparison.OrdinalIgnoreCase);
        }

        bool startDateChanged = currentStartDate != validatedData.StartDate;
        bool endDateChanged = currentEndDate != validatedData.EndDate;
        bool datesChanged = startDateChanged || endDateChanged;

        if (nameChanged || datesChanged)
        {
            List<Weeks> weeks = await _weekRepository.GetAllAsync();

            if (nameChanged)
            {
                foreach (Weeks existingWeek in weeks)
                {
                    if (existingWeek.Id != dto.Id)
                    {
                        if (existingWeek.Name != null)
                        {
                            if (string.Equals(existingWeek.Name.Trim(), validatedData.Name, StringComparison.OrdinalIgnoreCase))
                            {
                                return Result<GetWeekDto>.Failure("Название недели уже используется другой учебной неделей");
                            }
                        }
                    }
                }
            }

            if (datesChanged)
            {
                foreach (Weeks existingWeek in weeks)
                {
                    if (existingWeek.Id != dto.Id)
                    {
                        if (HasDateIntersection(existingWeek, validatedData.StartDate, validatedData.EndDate))
                        {
                            return Result<GetWeekDto>.Failure("Указанный временной период пересекается с уже существующей неделей");
                        }
                    }
                }
            }
        }

        week.Name = validatedData.Name;
        week.StartDate = validatedData.StartDate.ToDateTime(TimeOnly.MinValue);
        week.EndDate = validatedData.EndDate.ToDateTime(TimeOnly.MinValue);

        await _weekRepository.UpdateAsync(week);

        GetWeekDto mappedDto = MapToGetWeekDto(week);

        return Result<GetWeekDto>.Success(mappedDto);
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

    private static Result<ValidatedWeekData> ValidateWeekData(
        string name,
        DateOnly startDate,
        DateOnly endDate)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result<ValidatedWeekData>.Failure("Название недели не может быть пустым");
        }

        string trimmedName = name.Trim();

        if (trimmedName.Length < 2)
        {
            return Result<ValidatedWeekData>.Failure("Длина названия недели должна быть от 2 до 50 символов");
        }

        if (trimmedName.Length > 50)
        {
            return Result<ValidatedWeekData>.Failure("Длина названия недели должна быть от 2 до 50 символов");
        }

        if (ContainsDangerousCharacters(trimmedName))
        {
            return Result<ValidatedWeekData>.Failure("Обнаружены недопустимые символы в названии недели");
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
            return Result<ValidatedWeekData>.Failure("Дата начала недели не может быть позже или равна дате окончания");
        }

        ValidatedWeekData data = new ValidatedWeekData
        {
            Name = trimmedName,
            StartDate = startDate,
            EndDate = endDate
        };

        return Result<ValidatedWeekData>.Success(data);
    }

    private static GetWeekDto MapToGetWeekDto(Weeks week)
    {
        GetWeekDto dto = new GetWeekDto
        {
            Id = week.Id,
            Name = week.Name ?? string.Empty,
            StartDate = ConvertToDateOnly(week.StartDate),
            EndDate = ConvertToDateOnly(week.EndDate)
        };

        return dto;
    }

    private static bool HasDateIntersection(
        Weeks existingWeek,
        DateOnly startDate,
        DateOnly endDate)
    {
        DateOnly existingStartDate = ConvertToDateOnly(existingWeek.StartDate);
        DateOnly existingEndDate = ConvertToDateOnly(existingWeek.EndDate);

        if (existingStartDate == default)
        {
            return false;
        }

        if (existingEndDate == default)
        {
            return false;
        }

        if (startDate < existingEndDate)
        {
            if (endDate > existingStartDate)
            {
                return true;
            }
        }

        return false;
    }

    private static DateOnly ConvertToDateOnly(DateTime? dateTime)
    {
        if (dateTime == null)
        {
            return default;
        }

        DateOnly dateOnly = DateOnly.FromDateTime(dateTime.Value);

        return dateOnly;
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

    private class ValidatedWeekData
    {
        public string Name { get; set; } = null!;
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
    }
}
