using Application.Common;
using Application.DTOs.AudienceDTOs;
using Application.Interfaces.Interface;
using Application.Interfaces.Repository;
using Domain.Entities;
using Domain.Enums;

namespace Application.Service;

public class AudienceService : IAudienceService
{
    private readonly IAudienceRepository _audienceRepository;

    public AudienceService(IAudienceRepository audienceRepository)
    {
        _audienceRepository = audienceRepository;
    }

    public async Task<Result<GetAudienceDto>> GetByIdAsync(int id)
    {
        if (id <= 0)
        {
            return Result<GetAudienceDto>.Failure("Некорректный идентификатор аудитории");
        }

        Audience? audience = await _audienceRepository.GetByIdAsync(id);

        if (audience == null)
        {
            return Result<GetAudienceDto>.Failure("Аудитория не найдена");
        }

        GetAudienceDto mappedDto = MapToGetAudienceDto(audience);

        return Result<GetAudienceDto>.Success(mappedDto);
    }

    public async Task<Result<List<GetAudienceDto>>> GetAllAsync()
    {
        List<Audience> audiences = await _audienceRepository.GetAllAsync();

        List<GetAudienceDto> mappedDtos = new List<GetAudienceDto>();

        foreach (Audience audience in audiences)
        {
            GetAudienceDto mappedDto = MapToGetAudienceDto(audience);
            mappedDtos.Add(mappedDto);
        }

        return Result<List<GetAudienceDto>>.Success(mappedDtos);
    }

    public async Task<Result<GetAudienceDto>> CreateAsync(CreateAudienceDto dto)
    {
        if (dto == null)
        {
            return Result<GetAudienceDto>.Failure("Данные запроса отсутствуют");
        }

        Result<ValidatedAudienceData> validationResult = ValidateAudienceData(
            dto.Number,
            dto.Type);

        if (validationResult.IsFailure)
        {
            return Result<GetAudienceDto>.Failure(validationResult.Error);
        }

        ValidatedAudienceData validatedData = validationResult.Value;

        List<Audience> audiences = await _audienceRepository.GetAllAsync();

        foreach (Audience existingAudience in audiences)
        {
            if (string.Equals(existingAudience.Number.Trim(), validatedData.Number, StringComparison.OrdinalIgnoreCase))
            {
                return Result<GetAudienceDto>.Failure("Аудитория с таким номером уже существует");
            }
        }

        Audience audience = new Audience
        {
            Number = validatedData.Number,
            Type = validatedData.Type
        };

        await _audienceRepository.AddAsync(audience);

        GetAudienceDto mappedDto = MapToGetAudienceDto(audience);

        return Result<GetAudienceDto>.Success(mappedDto);
    }

    public async Task<Result<GetAudienceDto>> UpdateAsync(UpdateAudienceDto dto)
    {
        if (dto == null)
        {
            return Result<GetAudienceDto>.Failure("Данные запроса отсутствуют");
        }

        if (dto.Id <= 0)
        {
            return Result<GetAudienceDto>.Failure("Некорректный идентификатор аудитории");
        }

        Result<ValidatedAudienceData> validationResult = ValidateAudienceData(
            dto.Number,
            dto.Type);

        if (validationResult.IsFailure)
        {
            return Result<GetAudienceDto>.Failure(validationResult.Error);
        }

        ValidatedAudienceData validatedData = validationResult.Value;

        Audience? audience = await _audienceRepository.GetByIdAsync(dto.Id);

        if (audience == null)
        {
            return Result<GetAudienceDto>.Failure("Аудитория для обновления не найдена");
        }

        bool numberChanged = !string.Equals(audience.Number.Trim(), validatedData.Number, StringComparison.OrdinalIgnoreCase);

        if (numberChanged)
        {
            List<Audience> audiences = await _audienceRepository.GetAllAsync();

            foreach (Audience existingAudience in audiences)
            {
                if (existingAudience.Id != dto.Id)
                {
                    if (string.Equals(existingAudience.Number.Trim(), validatedData.Number, StringComparison.OrdinalIgnoreCase))
                    {
                        return Result<GetAudienceDto>.Failure("Аудитория с таким номером уже существует");
                    }
                }
            }
        }

        audience.Number = validatedData.Number;
        audience.Type = validatedData.Type;

        await _audienceRepository.UpdateAsync(audience);

        GetAudienceDto mappedDto = MapToGetAudienceDto(audience);

        return Result<GetAudienceDto>.Success(mappedDto);
    }

    public async Task<Result> DeleteAsync(int id)
    {
        if (id <= 0)
        {
            return Result.Failure("Некорректный идентификатор аудитории");
        }

        Audience? audience = await _audienceRepository.GetByIdAsync(id);

        if (audience == null)
        {
            return Result.Failure("Аудитория не найдена");
        }

        await _audienceRepository.DeleteAsync(audience);

        return Result.Success();
    }

    private static Result<ValidatedAudienceData> ValidateAudienceData(string number, string? type)
    {
        if (string.IsNullOrWhiteSpace(number))
        {
            return Result<ValidatedAudienceData>.Failure("Номер аудитории не может быть пустым");
        }

        string trimmedNumber = number.Trim();

        if (trimmedNumber.Length < 1)
        {
            return Result<ValidatedAudienceData>.Failure("Длина номера аудитории должна быть от 1 до 20 символов");
        }

        if (trimmedNumber.Length > 20)
        {
            return Result<ValidatedAudienceData>.Failure("Длина номера аудитории должна быть от 1 до 20 символов");
        }

        if (ContainsDangerousCharacters(trimmedNumber))
        {
            return Result<ValidatedAudienceData>.Failure("Обнаружены недопустимые символы в номере аудитории");
        }

        AudienceType audienceType = AudienceType.GeneralHall;

        if (!string.IsNullOrWhiteSpace(type))
        {
            string trimmedType = type.Trim();

            if (trimmedType.Length > 50)
            {
                return Result<ValidatedAudienceData>.Failure("Длина типа аудитории не должна превышать 50 символов");
            }

            if (ContainsDangerousCharacters(trimmedType))
            {
                return Result<ValidatedAudienceData>.Failure("Обнаружены недопустимые символы в типе аудитории");
            }

            Result<AudienceType> typeResult = ParseAudienceType(trimmedType);

            if (typeResult.IsFailure)
            {
                return Result<ValidatedAudienceData>.Failure(typeResult.Error);
            }

            audienceType = typeResult.Value;
        }

        ValidatedAudienceData data = new ValidatedAudienceData
        {
            Number = trimmedNumber,
            Type = audienceType
        };

        return Result<ValidatedAudienceData>.Success(data);
    }

    private static Result<AudienceType> ParseAudienceType(string value)
    {
        if (string.Equals(value, "ComputerLab", StringComparison.OrdinalIgnoreCase))
        {
            return Result<AudienceType>.Success(AudienceType.ComputerLab);
        }

        if (string.Equals(value, "Компьютерный класс", StringComparison.OrdinalIgnoreCase))
        {
            return Result<AudienceType>.Success(AudienceType.ComputerLab);
        }

        if (string.Equals(value, "Компьютерный кабинет", StringComparison.OrdinalIgnoreCase))
        {
            return Result<AudienceType>.Success(AudienceType.ComputerLab);
        }

        if (string.Equals(value, "ChemistryLab", StringComparison.OrdinalIgnoreCase))
        {
            return Result<AudienceType>.Success(AudienceType.ChemistryLab);
        }

        if (string.Equals(value, "Лаборатория химии", StringComparison.OrdinalIgnoreCase))
        {
            return Result<AudienceType>.Success(AudienceType.ChemistryLab);
        }

        if (string.Equals(value, "GeologyLab", StringComparison.OrdinalIgnoreCase))
        {
            return Result<AudienceType>.Success(AudienceType.GeologyLab);
        }

        if (string.Equals(value, "Лаборатория геологии", StringComparison.OrdinalIgnoreCase))
        {
            return Result<AudienceType>.Success(AudienceType.GeologyLab);
        }

        if (string.Equals(value, "PhysicsLab", StringComparison.OrdinalIgnoreCase))
        {
            return Result<AudienceType>.Success(AudienceType.PhysicsLab);
        }

        if (string.Equals(value, "Лаборатория физики", StringComparison.OrdinalIgnoreCase))
        {
            return Result<AudienceType>.Success(AudienceType.PhysicsLab);
        }

        if (string.Equals(value, "LectureHall", StringComparison.OrdinalIgnoreCase))
        {
            return Result<AudienceType>.Success(AudienceType.LectureHall);
        }

        if (string.Equals(value, "Лекционная", StringComparison.OrdinalIgnoreCase))
        {
            return Result<AudienceType>.Success(AudienceType.LectureHall);
        }

        if (string.Equals(value, "Лекционная аудитория", StringComparison.OrdinalIgnoreCase))
        {
            return Result<AudienceType>.Success(AudienceType.LectureHall);
        }

        if (string.Equals(value, "StreamHall", StringComparison.OrdinalIgnoreCase))
        {
            return Result<AudienceType>.Success(AudienceType.StreamHall);
        }

        if (string.Equals(value, "Потоковая аудитория", StringComparison.OrdinalIgnoreCase))
        {
            return Result<AudienceType>.Success(AudienceType.StreamHall);
        }

        if (string.Equals(value, "GeneralHall", StringComparison.OrdinalIgnoreCase))
        {
            return Result<AudienceType>.Success(AudienceType.GeneralHall);
        }

        if (string.Equals(value, "Общая аудитория", StringComparison.OrdinalIgnoreCase))
        {
            return Result<AudienceType>.Success(AudienceType.GeneralHall);
        }

        return Result<AudienceType>.Failure("Некорректный тип аудитории");
    }

    private static GetAudienceDto MapToGetAudienceDto(Audience audience)
    {
        GetAudienceDto dto = new GetAudienceDto
        {
            Id = audience.Id,
            Number = audience.Number,
            Type = audience.Type.ToString()
        };

        return dto;
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

    private class ValidatedAudienceData
    {
        public string Number { get; set; } = null!;
        public AudienceType Type { get; set; }
    }
}
