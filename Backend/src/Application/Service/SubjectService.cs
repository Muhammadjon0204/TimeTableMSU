using Application.Common;
using Application.DTOs.SubjectDTOs;
using Application.Interfaces.Interface;
using Application.Interfaces.Repository;
using Domain.Entities;
using Domain.Enums;

namespace Application.Service;

public class SubjectService : ISubjectService
{
    private readonly ISubjectRepository _subjectRepository;

    public SubjectService(ISubjectRepository subjectRepository)
    {
        _subjectRepository = subjectRepository;
    }

    public async Task<Result<GetSubjectDto>> GetByIdAsync(int id)
    {
        if (id <= 0)
        {
            return Result<GetSubjectDto>.Failure("Некорректный идентификатор предмета");
        }

        Subject? subject = await _subjectRepository.GetByIdAsync(id);

        if (subject == null)
        {
            return Result<GetSubjectDto>.Failure("Предмет не найден");
        }

        GetSubjectDto mappedDto = MapToGetSubjectDto(subject);

        return Result<GetSubjectDto>.Success(mappedDto);
    }

    public async Task<Result<List<GetSubjectDto>>> GetAllAsync()
    {
        List<Subject> subjects = await _subjectRepository.GetAllAsync();

        List<GetSubjectDto> mappedDtos = new List<GetSubjectDto>();

        foreach (Subject subject in subjects)
        {
            GetSubjectDto mappedDto = MapToGetSubjectDto(subject);
            mappedDtos.Add(mappedDto);
        }

        return Result<List<GetSubjectDto>>.Success(mappedDtos);
    }

    public async Task<Result<GetSubjectDto>> CreateAsync(CreateSubjectDto dto)
    {
        if (dto == null)
        {
            return Result<GetSubjectDto>.Failure("Данные запроса отсутствуют");
        }

        Result<ValidatedSubjectData> validationResult = ValidateSubjectData(
            dto.Name,
            dto.Semester,
            dto.HourCount,
            dto.ControlForm);

        if (validationResult.IsFailure)
        {
            return Result<GetSubjectDto>.Failure(validationResult.Error);
        }

        ValidatedSubjectData validatedData = validationResult.Value;

        List<Subject> subjects = await _subjectRepository.GetAllAsync();

        foreach (Subject existingSubject in subjects)
        {
            if (existingSubject.Semester == validatedData.Semester)
            {
                if (string.Equals(existingSubject.Name.Trim(), validatedData.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return Result<GetSubjectDto>.Failure("Предмет с таким названием уже существует в указанном семестре");
                }
            }
        }

        Subject subject = new Subject
        {
            Name = validatedData.Name,
            Semester = validatedData.Semester,
            HourCount = validatedData.HourCount,
            ControlForm = validatedData.ControlForm
        };

        await _subjectRepository.AddAsync(subject);

        GetSubjectDto mappedDto = MapToGetSubjectDto(subject);

        return Result<GetSubjectDto>.Success(mappedDto);
    }

    public async Task<Result<GetSubjectDto>> UpdateAsync(UpdateSubjectDto dto)
    {
        if (dto == null)
        {
            return Result<GetSubjectDto>.Failure("Данные запроса отсутствуют");
        }

        if (dto.Id <= 0)
        {
            return Result<GetSubjectDto>.Failure("Некорректный идентификатор предмета");
        }

        Result<ValidatedSubjectData> validationResult = ValidateSubjectData(
            dto.Name,
            dto.Semester,
            dto.HourCount,
            dto.ControlForm);

        if (validationResult.IsFailure)
        {
            return Result<GetSubjectDto>.Failure(validationResult.Error);
        }

        ValidatedSubjectData validatedData = validationResult.Value;

        Subject? subject = await _subjectRepository.GetByIdAsync(dto.Id);

        if (subject == null)
        {
            return Result<GetSubjectDto>.Failure("Предмет не найден");
        }

        List<Subject> subjects = await _subjectRepository.GetAllAsync();

        foreach (Subject existingSubject in subjects)
        {
            if (existingSubject.Id != dto.Id)
            {
                if (existingSubject.Semester == validatedData.Semester)
                {
                    if (string.Equals(existingSubject.Name.Trim(), validatedData.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        return Result<GetSubjectDto>.Failure("Название уже используется другим предметом в указанном семестре");
                    }
                }
            }
        }

        subject.Name = validatedData.Name;
        subject.Semester = validatedData.Semester;
        subject.HourCount = validatedData.HourCount;
        subject.ControlForm = validatedData.ControlForm;

        await _subjectRepository.UpdateAsync(subject);

        GetSubjectDto mappedDto = MapToGetSubjectDto(subject);

        return Result<GetSubjectDto>.Success(mappedDto);
    }

    public async Task<Result> DeleteAsync(int id)
    {
        if (id <= 0)
        {
            return Result.Failure("Некорректный идентификатор предмета");
        }

        Subject? subject = await _subjectRepository.GetByIdAsync(id);

        if (subject == null)
        {
            return Result.Failure("Предмет не найден");
        }

        await _subjectRepository.DeleteAsync(subject);

        return Result.Success();
    }

    private static Result<ValidatedSubjectData> ValidateSubjectData(
        string name,
        short? semester,
        int? hourCount,
        ControlForm controlForm)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result<ValidatedSubjectData>.Failure("Название предмета не может быть пустым");
        }

        string trimmedName = name.Trim();

        if (trimmedName.Length < 3)
        {
            return Result<ValidatedSubjectData>.Failure("Длина названия предмета должна быть от 3 до 150 символов");
        }

        if (trimmedName.Length > 150)
        {
            return Result<ValidatedSubjectData>.Failure("Длина названия предмета должна быть от 3 до 150 символов");
        }

        if (ContainsDangerousCharacters(trimmedName))
        {
            return Result<ValidatedSubjectData>.Failure("Обнаружены недопустимые символы в названии предмета");
        }

        if (semester == null)
        {
            return Result<ValidatedSubjectData>.Failure("Некорректный номер семестра");
        }

        if (semester < 1)
        {
            return Result<ValidatedSubjectData>.Failure("Некорректный номер семестра");
        }

        if (semester > 12)
        {
            return Result<ValidatedSubjectData>.Failure("Некорректный номер семестра");
        }

        if (hourCount == null)
        {
            return Result<ValidatedSubjectData>.Failure("Некорректное количество часов");
        }

        if (hourCount <= 0)
        {
            return Result<ValidatedSubjectData>.Failure("Некорректное количество часов");
        }

        if (hourCount > 500)
        {
            return Result<ValidatedSubjectData>.Failure("Количество часов не может превышать 500");
        }

        if (!Enum.IsDefined(typeof(ControlForm), controlForm))
        {
            return Result<ValidatedSubjectData>.Failure("Недопустимая форма контроля");
        }

        if (controlForm != ControlForm.Exam)
        {
            if (controlForm != ControlForm.Credit)
            {
                return Result<ValidatedSubjectData>.Failure("Недопустимая форма контроля");
            }
        }

        ValidatedSubjectData data = new ValidatedSubjectData
        {
            Name = trimmedName,
            Semester = semester.Value,
            HourCount = hourCount.Value,
            ControlForm = controlForm
        };

        return Result<ValidatedSubjectData>.Success(data);
    }

    private static GetSubjectDto MapToGetSubjectDto(Subject subject)
    {
        GetSubjectDto dto = new GetSubjectDto
        {
            Id = subject.Id,
            Name = subject.Name,
            ControlForm = subject.ControlForm
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

    private class ValidatedSubjectData
    {
        public string Name { get; set; } = null!;
        public short Semester { get; set; }
        public int HourCount { get; set; }
        public ControlForm ControlForm { get; set; }
    }
}
