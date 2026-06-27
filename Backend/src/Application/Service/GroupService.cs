using Application.Common;
using Application.DTOs.GroupDTOs;
using Application.Interfaces.Interface;
using Application.Interfaces.Repository;
using Domain.Entities;

namespace Application.Service;

public class GroupService : IGroupService
{
    private readonly IGroupRepository _groupRepository;
    private readonly ISpecialityRepository _specialityRepository;

    public GroupService(IGroupRepository groupRepository, ISpecialityRepository specialityRepository)
    {
        _groupRepository = groupRepository;
        _specialityRepository = specialityRepository;
    }

    public async Task<Result<GetGroupDetailDto>> GetByIdAsync(int id)
    {
        if (id <= 0)
        {
            return Result<GetGroupDetailDto>.Failure("Некорректный идентификатор группы");
        }

        Group? group = await _groupRepository.GetByIdAsync(id);

        if (group == null)
        {
            return Result<GetGroupDetailDto>.Failure("Группа не найдена");
        }

        string? specialityName = group.Speciality?.Name;

        if (string.IsNullOrWhiteSpace(specialityName))
        {
            Speciality? speciality = await _specialityRepository.GetByIdAsync(group.SpecialityId);

            if (speciality != null)
            {
                specialityName = speciality.Name;
            }
        }

        GetGroupDetailDto mappedDto = MapToGetGroupDetailDto(group, specialityName);

        return Result<GetGroupDetailDto>.Success(mappedDto);
    }

    public async Task<Result<List<GetGroupDto>>> GetAllAsync()
    {
        List<Group> groups = await _groupRepository.GetAllAsync();

        if (groups.Count == 0)
        {
            return Result<List<GetGroupDto>>.Success(new List<GetGroupDto>());
        }

        List<GetGroupDto> mappedDtos = new List<GetGroupDto>();

        foreach (Group group in groups)
        {
            string? specialityName = group.Speciality?.Name;

            if (string.IsNullOrWhiteSpace(specialityName))
            {
                Speciality? speciality = await _specialityRepository.GetByIdAsync(group.SpecialityId);

                if (speciality != null)
                {
                    specialityName = speciality.Name;
                }
            }

            GetGroupDto mappedDto = MapToGetGroupDto(group, specialityName);
            mappedDtos.Add(mappedDto);
        }

        return Result<List<GetGroupDto>>.Success(mappedDtos);
    }

    public async Task<Result<GetGroupDto>> CreateAsync(CreateGroupDto dto)
    {
        if (dto == null)
        {
            return Result<GetGroupDto>.Failure("Данные запроса отсутствуют");
        }

        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return Result<GetGroupDto>.Failure("Название группы не может быть пустым");
        }

        string trimmedName = dto.Name.Trim();

        if (trimmedName.Length < 2)
        {
            return Result<GetGroupDto>.Failure("Длина названия группы должна быть от 2 до 30 символов");
        }

        if (trimmedName.Length > 30)
        {
            return Result<GetGroupDto>.Failure("Длина названия группы должна быть от 2 до 30 символов");
        }

        if (ContainsDangerousCharacters(trimmedName))
        {
            return Result<GetGroupDto>.Failure("Обнаружены недопустимые символы в названии группы");
        }

        if (dto.Course == null)
        {
            return Result<GetGroupDto>.Failure("Некорректный номер курса");
        }

        if (dto.Course < 1)
        {
            return Result<GetGroupDto>.Failure("Некорректный номер курса");
        }

        if (dto.Course > 6)
        {
            return Result<GetGroupDto>.Failure("Некорректный номер курса");
        }

        if (dto.SpecialityId <= 0)
        {
            return Result<GetGroupDto>.Failure("Некорректный идентификатор специальности");
        }

        Speciality? speciality = await _specialityRepository.GetByIdAsync(dto.SpecialityId);

        if (speciality == null)
        {
            return Result<GetGroupDto>.Failure("Указанная специальность не существует");
        }

        List<Group> groups = await _groupRepository.GetAllAsync();

        foreach (Group existingGroup in groups)
        {
            if (existingGroup.SpecialityId == dto.SpecialityId)
            {
                if (string.Equals(existingGroup.Name.Trim(), trimmedName, StringComparison.OrdinalIgnoreCase))
                {
                    return Result<GetGroupDto>.Failure("Группа с таким названием уже существует в указанной специальности");
                }
            }
        }

        Group group = new Group
        {
            Name = trimmedName,
            SpecialityId = dto.SpecialityId,
            Course = dto.Course.Value,
            Speciality = speciality
        };

        await _groupRepository.AddAsync(group);

        GetGroupDto mappedDto = MapToGetGroupDto(group, speciality.Name);

        return Result<GetGroupDto>.Success(mappedDto);
    }

    public async Task<Result<GetGroupDto>> UpdateAsync(UpdateGroupDto dto)
    {
        if (dto == null)
        {
            return Result<GetGroupDto>.Failure("Данные запроса отсутствуют");
        }

        if (dto.Id <= 0)
        {
            return Result<GetGroupDto>.Failure("Некорректный идентификатор группы");
        }

        if (dto.SpecialityId <= 0)
        {
            return Result<GetGroupDto>.Failure("Некорректный идентификатор специальности");
        }

        if (dto.Course == null)
        {
            return Result<GetGroupDto>.Failure("Некорректный номер курса");
        }

        if (dto.Course < 1)
        {
            return Result<GetGroupDto>.Failure("Некорректный номер курса");
        }

        if (dto.Course > 6)
        {
            return Result<GetGroupDto>.Failure("Некорректный номер курса");
        }

        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return Result<GetGroupDto>.Failure("Название группы не может быть пустым");
        }

        string trimmedName = dto.Name.Trim();

        if (trimmedName.Length < 2)
        {
            return Result<GetGroupDto>.Failure("Длина названия группы должна быть от 2 до 30 символов");
        }

        if (trimmedName.Length > 30)
        {
            return Result<GetGroupDto>.Failure("Длина названия группы должна быть от 2 до 30 символов");
        }

        if (ContainsDangerousCharacters(trimmedName))
        {
            return Result<GetGroupDto>.Failure("Обнаружены недопустимые символы в названии группы");
        }

        Group? group = await _groupRepository.GetByIdAsync(dto.Id);

        if (group == null)
        {
            return Result<GetGroupDto>.Failure("Группа для обновления не найдена");
        }

        Speciality? speciality = await _specialityRepository.GetByIdAsync(dto.SpecialityId);

        if (speciality == null)
        {
            return Result<GetGroupDto>.Failure("Указанная специальность не существует");
        }

        List<Group> groups = await _groupRepository.GetAllAsync();

        foreach (Group existingGroup in groups)
        {
            if (existingGroup.Id != dto.Id)
            {
                if (existingGroup.SpecialityId == dto.SpecialityId)
                {
                    if (string.Equals(existingGroup.Name.Trim(), trimmedName, StringComparison.OrdinalIgnoreCase))
                    {
                        return Result<GetGroupDto>.Failure("Название уже используется другой группой в указанной специальности");
                    }
                }
            }
        }

        group.Name = trimmedName;
        group.SpecialityId = dto.SpecialityId;
        group.Course = dto.Course.Value;
        group.Speciality = speciality;

        await _groupRepository.UpdateAsync(group);

        GetGroupDto mappedDto = MapToGetGroupDto(group, speciality.Name);

        return Result<GetGroupDto>.Success(mappedDto);
    }

    public async Task<Result> DeleteAsync(int id)
    {
        if (id <= 0)
        {
            return Result.Failure("Некорректный идентификатор группы");
        }

        Group? group = await _groupRepository.GetByIdAsync(id);

        if (group == null)
        {
            return Result.Failure("Группа не найдена");
        }

        await _groupRepository.DeleteAsync(group);

        return Result.Success();
    }

    private static GetGroupDto MapToGetGroupDto(Group group, string? specialityName)
    {
        GetGroupDto dto = new GetGroupDto
        {
            Id = group.Id,
            Name = group.Name,
            Course = group.Course,
            SpecialityName = specialityName
        };

        return dto;
    }

    private static GetGroupDetailDto MapToGetGroupDetailDto(Group group, string? specialityName)
    {
        GetGroupDetailDto dto = new GetGroupDetailDto
        {
            Id = group.Id,
            Name = group.Name,
            SpecialityId = group.SpecialityId,
            Course = group.Course,
            SpecialityName = specialityName
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
}
