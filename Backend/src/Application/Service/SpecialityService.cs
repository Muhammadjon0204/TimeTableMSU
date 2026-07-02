using Application.Common;
using Application.DTOs.SpecialityDTOs;
using Application.Interfaces.Interface;
using Application.Interfaces.Repository;
using Domain.Entities;

namespace Application.Service;

public class SpecialityService : ISpecialityService
{
    private readonly ISpecialityRepository _specialityRepository;
    private readonly IFacultyRepository _facultyRepository;

    public SpecialityService(ISpecialityRepository specialityRepository, IFacultyRepository facultyRepository)
    {
        _specialityRepository = specialityRepository;
        _facultyRepository = facultyRepository;
    }

    public async Task<Result<GetSpecialityDto>> GetByIdAsync(int id)
    {
        if (id <= 0)
        {
            return Result<GetSpecialityDto>.Failure("Некорректный идентификатор специальности");
        }

        Speciality? speciality = await _specialityRepository.GetByIdAsync(id);

        if (speciality == null)
        {
            return Result<GetSpecialityDto>.Failure("Специальность не найдена");
        }

        GetSpecialityDto mappedDto = MapToGetSpecialityDto(speciality);

        return Result<GetSpecialityDto>.Success(mappedDto);
    }

    public async Task<Result<List<GetSpecialityDto>>> GetAllAsync()
    {
        List<Speciality> specialities = await _specialityRepository.GetAllAsync();

        List<GetSpecialityDto> mappedDtos = new List<GetSpecialityDto>();

        foreach (Speciality speciality in specialities)
        {
            GetSpecialityDto mappedDto = MapToGetSpecialityDto(speciality);
            mappedDtos.Add(mappedDto);
        }

        return Result<List<GetSpecialityDto>>.Success(mappedDtos);
    }

    public async Task<Result<GetSpecialityDto>> CreateAsync(CreateSpecialityDto dto)
    {
        if (dto == null)
        {
            return Result<GetSpecialityDto>.Failure("Данные запроса отсутствуют");
        }

        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return Result<GetSpecialityDto>.Failure("Название специальности не может быть пустым");
        }

        string trimmedName = dto.Name.Trim();

        if (trimmedName.Length < 2)
        {
            return Result<GetSpecialityDto>.Failure("Длина названия специальности должна быть от 2 до 150 символов");
        }

        if (trimmedName.Length > 150)
        {
            return Result<GetSpecialityDto>.Failure("Длина названия специальности должна быть от 2 до 150 символов");
        }

        if (!IsValidSpecialityName(trimmedName))
        {
            return Result<GetSpecialityDto>.Failure("Обнаружены недопустимые символы");
        }

        if (dto.FacultyId <= 0)
        {
            return Result<GetSpecialityDto>.Failure("Некорректный идентификатор факультета");
        }

        Faculty? faculty = await _facultyRepository.GetByIdAsync(dto.FacultyId);

        if (faculty == null)
        {
            return Result<GetSpecialityDto>.Failure("Указанный факультет не существует");
        }

        List<Speciality> specialities = await _specialityRepository.GetAllAsync();

        foreach (Speciality existingSpeciality in specialities)
        {
            if (existingSpeciality.FacultyId == dto.FacultyId)
            {
                if (string.Equals(existingSpeciality.Name.Trim(), trimmedName, StringComparison.OrdinalIgnoreCase))
                {
                    return Result<GetSpecialityDto>.Failure("Специальность с таким названием уже существует на указанном факультете");
                }
            }
        }

        Speciality speciality = new Speciality
        {
            Name = trimmedName,
            FacultyId = dto.FacultyId,
            Faculty = faculty
        };

        await _specialityRepository.AddAsync(speciality);

        GetSpecialityDto mappedDto = MapToGetSpecialityDto(speciality);

        return Result<GetSpecialityDto>.Success(mappedDto);
    }

    public async Task<Result<GetSpecialityDto>> UpdateAsync(UpdateSpecialityDto dto)
    {
        if (dto == null)
        {
            return Result<GetSpecialityDto>.Failure("Данные запроса отсутствуют");
        }

        if (dto.Id <= 0)
        {
            return Result<GetSpecialityDto>.Failure("Некорректный идентификатор специальности");
        }

        if (dto.FacultyId <= 0)
        {
            return Result<GetSpecialityDto>.Failure("Некорректный идентификатор факультета");
        }

        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return Result<GetSpecialityDto>.Failure("Название специальности не может быть пустым");
        }

        string trimmedName = dto.Name.Trim();

        if (trimmedName.Length < 2)
        {
            return Result<GetSpecialityDto>.Failure("Длина названия специальности должна быть от 2 до 150 символов");
        }

        if (trimmedName.Length > 150)
        {
            return Result<GetSpecialityDto>.Failure("Длина названия специальности должна быть от 2 до 150 символов");
        }

        if (!IsValidSpecialityName(trimmedName))
        {
            return Result<GetSpecialityDto>.Failure("Обнаружены недопустимые символы");
        }

        Speciality? speciality = await _specialityRepository.GetByIdAsync(dto.Id);

        if (speciality == null)
        {
            return Result<GetSpecialityDto>.Failure("Специальность для обновления не найдена");
        }

        Faculty? faculty = await _facultyRepository.GetByIdAsync(dto.FacultyId);

        if (faculty == null)
        {
            return Result<GetSpecialityDto>.Failure("Указанный факультет не существует");
        }

        List<Speciality> specialities = await _specialityRepository.GetAllAsync();

        foreach (Speciality existingSpeciality in specialities)
        {
            if (existingSpeciality.Id != dto.Id)
            {
                if (existingSpeciality.FacultyId == dto.FacultyId)
                {
                    if (string.Equals(existingSpeciality.Name.Trim(), trimmedName, StringComparison.OrdinalIgnoreCase))
                    {
                        return Result<GetSpecialityDto>.Failure("Название уже используется другой специальностью на указанном факультете");
                    }
                }
            }
        }

        speciality.Name = trimmedName;
        speciality.FacultyId = dto.FacultyId;
        speciality.Faculty = faculty;

        await _specialityRepository.UpdateAsync(speciality);

        GetSpecialityDto mappedDto = MapToGetSpecialityDto(speciality);

        return Result<GetSpecialityDto>.Success(mappedDto);
    }

    public async Task<Result> DeleteAsync(int id)
    {
        if (id <= 0)
        {
            return Result.Failure("Некорректный идентификатор специальности");
        }

        Speciality? speciality = await _specialityRepository.GetByIdAsync(id);

        if (speciality == null)
        {
            return Result.Failure($"Специальность с ID {id} не найдена");
        }

        if (speciality.Groups.Count > 0)
        {
            return Result.Failure("Нельзя удалить специальность: сначала удалите или перенесите связанные группы.");
        }

        await _specialityRepository.DeleteAsync(speciality);

        return Result.Success();
    }

    private static GetSpecialityDto MapToGetSpecialityDto(Speciality speciality)
    {
        GetSpecialityDto dto = new GetSpecialityDto
        {
            Id = speciality.Id,
            Name = speciality.Name,
            FacultyId = speciality.FacultyId,
            FacultyName = speciality.Faculty.Name
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

    private static bool IsValidSpecialityName(string value)
    {
        if (ContainsDangerousCharacters(value))
        {
            return false;
        }

        foreach (char symbol in value)
        {
            if (char.IsLetter(symbol) || char.IsWhiteSpace(symbol) || symbol == '-' || symbol == '—' || symbol == '(' || symbol == ')')
            {
                continue;
            }

            return false;
        }

        return true;
    }
}
