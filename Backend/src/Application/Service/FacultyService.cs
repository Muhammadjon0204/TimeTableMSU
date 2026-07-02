using Application.Common;
using Application.DTOs.FacultyDTOs;
using Application.Interfaces.Interface;
using Application.Interfaces.Repository;
using Domain.Entities;

namespace Application.Service;

public class FacultyService : IFacultyService
{
    private readonly IFacultyRepository _facultyRepository;

    public FacultyService(IFacultyRepository facultyRepository)
    {
        _facultyRepository = facultyRepository;
    }

    public async Task<Result<GetFacultyDto>> GetByIdAsync(int id)
    {
        if (id <= 0)
        {
            return Result<GetFacultyDto>.Failure("Некорректный идентификатор факультета");
        }

        Faculty? faculty = await _facultyRepository.GetByIdAsync(id);

        if (faculty == null)
        {
            return Result<GetFacultyDto>.Failure("Факультет не найден");
        }

        GetFacultyDto mappedDto = MapToGetFacultyDto(faculty);

        return Result<GetFacultyDto>.Success(mappedDto);
    }

    public async Task<Result<List<GetFacultyDto>>> GetAllAsync()
    {
        List<Faculty> faculties = await _facultyRepository.GetAllAsync();

        List<GetFacultyDto> mappedDtos = new List<GetFacultyDto>();

        foreach (Faculty faculty in faculties)
        {
            GetFacultyDto mappedDto = MapToGetFacultyDto(faculty);
            mappedDtos.Add(mappedDto);
        }

        return Result<List<GetFacultyDto>>.Success(mappedDtos);
    }

    public async Task<Result<GetFacultyDto>> CreateAsync(CreateFacultyDto dto)
    {
        if (dto == null)
        {
            return Result<GetFacultyDto>.Failure("Данные запроса отсутствуют");
        }

        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return Result<GetFacultyDto>.Failure("Название факультета не может быть пустым");
        }

        string trimmedName = dto.Name.Trim();

        if (trimmedName.Length < 2)
        {
            return Result<GetFacultyDto>.Failure("Длина названия факультета должна быть от 2 до 100 символов");
        }

        if (trimmedName.Length > 100)
        {
            return Result<GetFacultyDto>.Failure("Длина названия факультета должна быть от 2 до 100 символов");
        }

        if (!IsValidFacultyName(trimmedName))
        {
            return Result<GetFacultyDto>.Failure("Обнаружены недопустимые символы в названии");
        }

        List<Faculty> faculties = await _facultyRepository.GetAllAsync();

        foreach (Faculty existingFaculty in faculties)
        {
            if (string.Equals(existingFaculty.Name.Trim(), trimmedName, StringComparison.OrdinalIgnoreCase))
            {
                return Result<GetFacultyDto>.Failure("Факультет с таким названием уже существует");
            }
        }

        Faculty faculty = new Faculty
        {
            Name = trimmedName
        };

        await _facultyRepository.AddAsync(faculty);

        GetFacultyDto mappedDto = MapToGetFacultyDto(faculty);

        return Result<GetFacultyDto>.Success(mappedDto);
    }

    public async Task<Result<GetFacultyDto>> UpdateAsync(UpdateFacultyDto dto)
    {
        if (dto == null)
        {
            return Result<GetFacultyDto>.Failure("Данные запроса отсутствуют");
        }

        if (dto.Id <= 0)
        {
            return Result<GetFacultyDto>.Failure("Некорректный идентификатор факультета");
        }

        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return Result<GetFacultyDto>.Failure("Название факультета не может быть пустым");
        }

        string trimmedName = dto.Name.Trim();

        if (trimmedName.Length < 2)
        {
            return Result<GetFacultyDto>.Failure("Длина названия факультета должна быть от 2 до 100 символов");
        }

        if (trimmedName.Length > 100)
        {
            return Result<GetFacultyDto>.Failure("Длина названия факультета должна быть от 2 до 100 символов");
        }

        if (!IsValidFacultyName(trimmedName))
        {
            return Result<GetFacultyDto>.Failure("Обнаружены недопустимые символы в названии");
        }

        Faculty? faculty = await _facultyRepository.GetByIdAsync(dto.Id);

        if (faculty == null)
        {
            return Result<GetFacultyDto>.Failure("Факультет для обновления не найден");
        }

        bool nameChanged = !string.Equals(faculty.Name.Trim(), trimmedName, StringComparison.OrdinalIgnoreCase);

        if (nameChanged)
        {
            List<Faculty> faculties = await _facultyRepository.GetAllAsync();

            foreach (Faculty existingFaculty in faculties)
            {
                if (existingFaculty.Id != dto.Id)
                {
                    if (string.Equals(existingFaculty.Name.Trim(), trimmedName, StringComparison.OrdinalIgnoreCase))
                    {
                        return Result<GetFacultyDto>.Failure("Название уже используется другим факультетом");
                    }
                }
            }
        }

        faculty.Name = trimmedName;

        await _facultyRepository.UpdateAsync(faculty);

        GetFacultyDto mappedDto = MapToGetFacultyDto(faculty);

        return Result<GetFacultyDto>.Success(mappedDto);
    }

    public async Task<Result> DeleteAsync(int id)
    {
        if (id <= 0)
        {
            return Result.Failure("Некорректный идентификатор факультета");
        }

        Faculty? faculty = await _facultyRepository.GetByIdAsync(id);

        if (faculty == null)
        {
            return Result.Failure($"Факультет с ID {id} не найден");
        }

        await _facultyRepository.DeleteAsync(faculty);

        return Result.Success();
    }

    private static GetFacultyDto MapToGetFacultyDto(Faculty faculty)
    {
        GetFacultyDto dto = new GetFacultyDto
        {
            Id = faculty.Id,
            Name = faculty.Name
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

    private static bool IsValidFacultyName(string value)
    {
        if (ContainsDangerousCharacters(value))
        {
            return false;
        }

        foreach (char symbol in value)
        {
            if (char.IsLetter(symbol) || char.IsWhiteSpace(symbol) || symbol == '-' || symbol == '—')
            {
                continue;
            }

            return false;
        }

        return true;
    }
}
