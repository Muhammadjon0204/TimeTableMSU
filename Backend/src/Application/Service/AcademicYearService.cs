using Application.Common;
using Application.DTOs.AcademicYearDTOs;
using Application.DTOs.WeekDTOs;
using Application.Interfaces.Interface;
using Application.Interfaces.Repository;
using Domain.Entities;

namespace Application.Service;

public class AcademicYearService : IAcademicYearService
{
    private readonly IAcademicYearRepository _academicYearRepository;
    private readonly IWeekRepository _weekRepository;

    public AcademicYearService(IAcademicYearRepository academicYearRepository, IWeekRepository weekRepository)
    {
        _academicYearRepository = academicYearRepository;
        _weekRepository = weekRepository;
    }

    public async Task<Result<List<AcademicYearDto>>> GetAllAsync()
    {
        List<AcademicYear> years = await _academicYearRepository.GetAllAsync();
        return Result<List<AcademicYearDto>>.Success(years.Select(MapAcademicYear).ToList());
    }

    public async Task<Result<AcademicYearDto>> GetCurrentAsync()
    {
        AcademicYear? year = await _academicYearRepository.GetCurrentAsync();

        if (year == null)
        {
            return Result<AcademicYearDto>.Failure("Учебный год не найден");
        }

        return Result<AcademicYearDto>.Success(MapAcademicYear(year));
    }

    public async Task<Result<AcademicYearDto>> GetByIdAsync(int id)
    {
        if (id <= 0)
        {
            return Result<AcademicYearDto>.Failure("Некорректный идентификатор учебного года");
        }

        AcademicYear? year = await _academicYearRepository.GetByIdAsync(id);

        if (year == null)
        {
            return Result<AcademicYearDto>.Failure("Учебный год не найден");
        }

        return Result<AcademicYearDto>.Success(MapAcademicYear(year));
    }

    public async Task<Result<AcademicYearDto>> CreateAsync(CreateAcademicYearDto dto)
    {
        Result<ValidatedAcademicYear> validation = await ValidateAcademicYearAsync(dto.Name, dto.StartDate, dto.EndDate, null);

        if (validation.IsFailure)
        {
            return Result<AcademicYearDto>.Failure(validation.Error);
        }

        ValidatedAcademicYear data = validation.Value;
        AcademicYear academicYear = new()
        {
            Name = data.Name,
            StartDate = data.StartDate.ToDateTime(TimeOnly.MinValue),
            EndDate = data.EndDate.ToDateTime(TimeOnly.MinValue),
            IsCurrent = dto.IsCurrent,
            CreatedAt = DateTime.UtcNow
        };

        await _academicYearRepository.AddAsync(academicYear);

        return Result<AcademicYearDto>.Success(MapAcademicYear(academicYear));
    }

    public async Task<Result<AcademicYearDto>> UpdateAsync(UpdateAcademicYearDto dto)
    {
        if (dto.Id <= 0)
        {
            return Result<AcademicYearDto>.Failure("Некорректный идентификатор учебного года");
        }

        AcademicYear? academicYear = await _academicYearRepository.GetByIdAsync(dto.Id);

        if (academicYear == null)
        {
            return Result<AcademicYearDto>.Failure("Учебный год не найден");
        }

        Result<ValidatedAcademicYear> validation = await ValidateAcademicYearAsync(dto.Name, dto.StartDate, dto.EndDate, dto.Id);

        if (validation.IsFailure)
        {
            return Result<AcademicYearDto>.Failure(validation.Error);
        }

        ValidatedAcademicYear data = validation.Value;
        academicYear.Name = data.Name;
        academicYear.StartDate = data.StartDate.ToDateTime(TimeOnly.MinValue);
        academicYear.EndDate = data.EndDate.ToDateTime(TimeOnly.MinValue);
        academicYear.IsCurrent = dto.IsCurrent;

        await _academicYearRepository.UpdateAsync(academicYear);

        return Result<AcademicYearDto>.Success(MapAcademicYear(academicYear));
    }

    public async Task<Result> DeleteAsync(int id)
    {
        AcademicYear? academicYear = await _academicYearRepository.GetByIdAsync(id);

        if (academicYear == null)
        {
            return Result.Failure("Учебный год не найден");
        }

        if (academicYear.Weeks.Count > 0)
        {
            return Result.Failure("Нельзя удалить учебный год, у которого уже есть недели");
        }

        await _academicYearRepository.DeleteAsync(academicYear);
        return Result.Success();
    }

    public async Task<Result<GenerateWeeksResponse>> GenerateWeeksAsync(int academicYearId, GenerateWeeksRequest request)
    {
        if (request.OverwriteExistingWeeks)
        {
            return Result<GenerateWeeksResponse>.Failure("Перезапись недель пока недоступна, так как недели могут быть связаны с расписанием.");
        }

        AcademicYear? academicYear = await _academicYearRepository.GetByIdAsync(academicYearId);

        if (academicYear == null)
        {
            return Result<GenerateWeeksResponse>.Failure("Учебный год не найден");
        }

        DateOnly academicYearStart = DateOnly.FromDateTime(academicYear.StartDate);
        DateOnly academicYearEnd = DateOnly.FromDateTime(academicYear.EndDate);
        DateOnly startDate = request.StartDate ?? academicYearStart;
        DateOnly generateUntil = request.GenerateUntil ?? AcademicWeekGenerator.GetDefaultGenerateUntil(academicYear);

        if (startDate < academicYearStart)
        {
            return Result<GenerateWeeksResponse>.Failure("Дата начала не может быть раньше начала учебного года");
        }

        if (generateUntil > academicYearEnd)
        {
            return Result<GenerateWeeksResponse>.Failure("Дата окончания генерации не может быть позже конца учебного года");
        }

        if (startDate > generateUntil)
        {
            return Result<GenerateWeeksResponse>.Failure("Дата начала не может быть позже даты окончания генерации");
        }

        List<Weeks> existingWeeks = await _weekRepository.GetByAcademicYearAsync(academicYearId);
        List<Weeks> generatedWeeks = AcademicWeekGenerator.GenerateStudyWeeks(academicYear, startDate, generateUntil);
        var newWeeks = new List<Weeks>();
        int skippedCount = 0;

        foreach (Weeks generatedWeek in generatedWeeks)
        {
            bool exists = existingWeeks.Any(existingWeek =>
                SameDate(existingWeek.StartDate, generatedWeek.StartDate) && SameDate(existingWeek.EndDate, generatedWeek.EndDate))
                || existingWeeks.Any(existingWeek => string.Equals(existingWeek.Name, generatedWeek.Name, StringComparison.OrdinalIgnoreCase))
                || newWeeks.Any(existingWeek =>
                    SameDate(existingWeek.StartDate, generatedWeek.StartDate) && SameDate(existingWeek.EndDate, generatedWeek.EndDate))
                || newWeeks.Any(existingWeek => string.Equals(existingWeek.Name, generatedWeek.Name, StringComparison.OrdinalIgnoreCase));

            if (exists)
            {
                skippedCount++;
                continue;
            }

            generatedWeek.AcademicYear = null;
            generatedWeek.AcademicYearId = academicYearId;
            newWeeks.Add(generatedWeek);
        }

        if (newWeeks.Count > 0)
        {
            await _weekRepository.AddRangeAsync(newWeeks);
        }

        List<GetWeekDto> allWeeks = (await _weekRepository.GetByAcademicYearAsync(academicYearId))
            .Select(MapWeek)
            .ToList();

        GenerateWeeksResponse response = new()
        {
            AcademicYearId = academicYear.Id,
            AcademicYearName = academicYear.Name,
            CreatedCount = newWeeks.Count,
            SkippedCount = skippedCount,
            StartDate = startDate,
            GenerateUntil = generateUntil,
            Weeks = allWeeks
        };

        return Result<GenerateWeeksResponse>.Success(response);
    }

    public async Task<Result<AcademicYearDto>> RecoverFromWeeksAsync()
    {
        List<AcademicYear> years = await _academicYearRepository.GetAllAsync();

        if (years.Any())
        {
            return Result<AcademicYearDto>.Failure("Academic years already exist");
        }

        List<Weeks> weeks = await _weekRepository.GetAllAsync();

        if (weeks == null || weeks.Count == 0)
        {
            return Result<AcademicYearDto>.Failure("Нет недель для восстановления учебного года");
        }

        DateTime minStart = weeks.Min(w => w.StartDate).Date;
        DateTime maxEnd = weeks.Max(w => w.EndDate).Date;

        int startYear = minStart.Month >= 9 ? minStart.Year : minStart.Year - 1;
        DateTime academicStart = new DateTime(startYear, 9, 1);
        DateTime academicEnd = new DateTime(startYear + 1, 8, 31);

        string name = $"{academicStart.Year}/{academicStart.Year + 1}";

        // Avoid duplicates by name
        if (await _academicYearRepository.NameExistsAsync(name))
        {
            return Result<AcademicYearDto>.Failure("Учебный год уже существует");
        }

        var academicYear = new AcademicYear
        {
            Name = name,
            StartDate = academicStart,
            EndDate = academicEnd,
            IsCurrent = true,
            CreatedAt = DateTime.UtcNow
        };

        await _academicYearRepository.AddAsync(academicYear);

        // Attach weeks that fall into this academic year and have no AcademicYearId
        var toAttach = weeks.Where(w => (!w.AcademicYearId.HasValue || w.AcademicYearId == 0) && w.StartDate.Date >= academicStart && w.EndDate.Date <= academicEnd).ToList();

        foreach (var w in toAttach)
        {
            w.AcademicYearId = academicYear.Id;
            await _weekRepository.UpdateAsync(w);
        }

        return Result<AcademicYearDto>.Success(MapAcademicYear(academicYear));
    }

    private async Task<Result<ValidatedAcademicYear>> ValidateAcademicYearAsync(string name, DateOnly startDate, DateOnly endDate, int? excludedId)
    {
        string trimmedName = (name ?? string.Empty).Trim();

        if (trimmedName.Length < 4 || trimmedName.Length > 20)
        {
            return Result<ValidatedAcademicYear>.Failure("Название учебного года должно быть от 4 до 20 символов");
        }

        if (startDate == default || endDate == default)
        {
            return Result<ValidatedAcademicYear>.Failure("Укажите даты начала и окончания учебного года");
        }

        if (startDate >= endDate)
        {
            return Result<ValidatedAcademicYear>.Failure("Дата начала учебного года должна быть раньше даты окончания");
        }

        if (await _academicYearRepository.NameExistsAsync(trimmedName, excludedId))
        {
            return Result<ValidatedAcademicYear>.Failure("Учебный год с таким названием уже существует");
        }

        return Result<ValidatedAcademicYear>.Success(new ValidatedAcademicYear(trimmedName, startDate, endDate));
    }

    private static AcademicYearDto MapAcademicYear(AcademicYear academicYear)
    {
        return new AcademicYearDto
        {
            Id = academicYear.Id,
            Name = academicYear.Name,
            StartDate = DateOnly.FromDateTime(academicYear.StartDate),
            EndDate = DateOnly.FromDateTime(academicYear.EndDate),
            IsCurrent = academicYear.IsCurrent,
            WeeksCount = academicYear.Weeks.Count
        };
    }

    private static GetWeekDto MapWeek(Weeks week)
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

    private static bool SameDate(DateTime first, DateTime second)
    {
        return first.Date == second.Date;
    }

    private sealed record ValidatedAcademicYear(string Name, DateOnly StartDate, DateOnly EndDate);
}
