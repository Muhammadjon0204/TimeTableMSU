using System.Text.RegularExpressions;
using Application.Common;
using Application.DTOs.TeacherDTOs;
using Application.Interfaces.Interface;
using Application.Interfaces.Repository;
using Domain.Entities;
using Domain.Enums;

namespace Application.Service;

public class TeacherService : ITeacherService
{
    private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);
    private static readonly Regex PhoneRegex = new(@"^\+?[1-9]\d{1,14}$", RegexOptions.Compiled);

    private readonly ITeacherRepository _teacherRepository;

    public TeacherService(ITeacherRepository teacherRepository)
    {
        _teacherRepository = teacherRepository;
    }

    public async Task<Result<GetTeacherDto>> GetByIdAsync(int id)
    {
        if (id <= 0)
        {
            return Result<GetTeacherDto>.Failure("Некорректный идентификатор преподавателя");
        }

        Teacher? teacher = await _teacherRepository.GetByIdAsync(id);

        if (teacher == null)
        {
            return Result<GetTeacherDto>.Failure("Преподаватель не найден");
        }

        GetTeacherDto mappedDto = MapToGetTeacherDto(teacher);

        return Result<GetTeacherDto>.Success(mappedDto);
    }

    public async Task<Result<List<GetTeacherDto>>> GetAllAsync()
    {
        List<Teacher> teachers = await _teacherRepository.GetAllAsync();

        List<GetTeacherDto> mappedDtos = new List<GetTeacherDto>();

        foreach (Teacher teacher in teachers)
        {
            GetTeacherDto mappedDto = MapToGetTeacherDto(teacher);
            mappedDtos.Add(mappedDto);
        }

        return Result<List<GetTeacherDto>>.Success(mappedDtos);
    }

    public async Task<Result<GetTeacherDto>> CreateAsync(CreateTeacherDto dto)
    {
        if (dto == null)
        {
            return Result<GetTeacherDto>.Failure("Данные запроса отсутствуют");
        }

        Result<ValidatedTeacherData> validationResult = ValidateTeacherData(
            dto.FirstName,
            dto.LastName,
            dto.FatherName,
            dto.TeacherDegree,
            dto.TeacherTitle,
            dto.TeacherPost,
            dto.Email,
            dto.Telephone);

        if (validationResult.IsFailure)
        {
            return Result<GetTeacherDto>.Failure(validationResult.Error);
        }

        ValidatedTeacherData validatedData = validationResult.Value;

        List<Teacher> teachers = await _teacherRepository.GetAllAsync();

        foreach (Teacher existingTeacher in teachers)
        {
            if (!string.IsNullOrWhiteSpace(validatedData.Email))
            {
                if (!string.IsNullOrWhiteSpace(existingTeacher.Email))
                {
                    if (string.Equals(existingTeacher.Email.Trim(), validatedData.Email, StringComparison.OrdinalIgnoreCase))
                    {
                        return Result<GetTeacherDto>.Failure("Преподаватель с таким Email уже существует");
                    }
                }
            }

            bool sameFirstName = string.Equals(existingTeacher.FirstName.Trim(), validatedData.FirstName, StringComparison.OrdinalIgnoreCase);
            bool sameLastName = string.Equals(existingTeacher.LastName.Trim(), validatedData.LastName, StringComparison.OrdinalIgnoreCase);
            bool sameFatherName = string.Equals((existingTeacher.FatherName ?? string.Empty).Trim(), validatedData.FatherName ?? string.Empty, StringComparison.OrdinalIgnoreCase);

            if (sameFirstName && sameLastName && sameFatherName)
            {
                return Result<GetTeacherDto>.Failure("Преподаватель с таким ФИО уже существует");
            }
        }

        Teacher teacher = new Teacher
        {
            FirstName = validatedData.FirstName,
            LastName = validatedData.LastName,
            FatherName = validatedData.FatherName,
            TeacherDegree = validatedData.TeacherDegree,
            TeacherTitle = validatedData.TeacherTitle,
            TeacherPost = validatedData.TeacherPost,
            Email = validatedData.Email,
            Telephone = validatedData.Telephone
        };

        await _teacherRepository.AddAsync(teacher);

        GetTeacherDto mappedDto = MapToGetTeacherDto(teacher);

        return Result<GetTeacherDto>.Success(mappedDto);
    }

    public async Task<Result<GetTeacherDto>> UpdateAsync(UpdateTeacherDto dto)
    {
        if (dto == null)
        {
            return Result<GetTeacherDto>.Failure("Данные запроса отсутствуют");
        }

        if (dto.Id <= 0)
        {
            return Result<GetTeacherDto>.Failure("Некорректный идентификатор преподавателя");
        }

        Result<ValidatedTeacherData> validationResult = ValidateTeacherData(
            dto.FirstName,
            dto.LastName,
            dto.FatherName,
            dto.TeacherDegree,
            dto.TeacherTitle,
            dto.TeacherPost,
            dto.Email,
            dto.Telephone);

        if (validationResult.IsFailure)
        {
            return Result<GetTeacherDto>.Failure(validationResult.Error);
        }

        ValidatedTeacherData validatedData = validationResult.Value;

        Teacher? teacher = await _teacherRepository.GetByIdAsync(dto.Id);

        if (teacher == null)
        {
            return Result<GetTeacherDto>.Failure("Преподаватель не найден");
        }

        List<Teacher> teachers = await _teacherRepository.GetAllAsync();

        foreach (Teacher existingTeacher in teachers)
        {
            if (existingTeacher.Id != dto.Id)
            {
                if (!string.IsNullOrWhiteSpace(validatedData.Email))
                {
                    if (!string.IsNullOrWhiteSpace(existingTeacher.Email))
                    {
                        if (string.Equals(existingTeacher.Email.Trim(), validatedData.Email, StringComparison.OrdinalIgnoreCase))
                        {
                            return Result<GetTeacherDto>.Failure("Email уже используется другим преподавателем");
                        }
                    }
                }

                bool sameFirstName = string.Equals(existingTeacher.FirstName.Trim(), validatedData.FirstName, StringComparison.OrdinalIgnoreCase);
                bool sameLastName = string.Equals(existingTeacher.LastName.Trim(), validatedData.LastName, StringComparison.OrdinalIgnoreCase);
                bool sameFatherName = string.Equals((existingTeacher.FatherName ?? string.Empty).Trim(), validatedData.FatherName ?? string.Empty, StringComparison.OrdinalIgnoreCase);

                if (sameFirstName && sameLastName && sameFatherName)
                {
                    return Result<GetTeacherDto>.Failure("Другой преподаватель с таким ФИО уже существует");
                }
            }
        }

        teacher.FirstName = validatedData.FirstName;
        teacher.LastName = validatedData.LastName;
        teacher.FatherName = validatedData.FatherName;
        teacher.TeacherDegree = validatedData.TeacherDegree;
        teacher.TeacherTitle = validatedData.TeacherTitle;
        teacher.TeacherPost = validatedData.TeacherPost;
        teacher.Email = validatedData.Email;
        teacher.Telephone = validatedData.Telephone;

        await _teacherRepository.UpdateAsync(teacher);

        GetTeacherDto mappedDto = MapToGetTeacherDto(teacher);

        return Result<GetTeacherDto>.Success(mappedDto);
    }

    public async Task<Result> DeleteAsync(int id)
    {
        if (id <= 0)
        {
            return Result.Failure("Некорректный идентификатор преподавателя");
        }

        Teacher? teacher = await _teacherRepository.GetByIdAsync(id);

        if (teacher == null)
        {
            return Result.Failure("Преподаватель не найден");
        }

        await _teacherRepository.DeleteAsync(teacher);

        return Result.Success();
    }

    private static Result<ValidatedTeacherData> ValidateTeacherData(
        string firstName,
        string lastName,
        string? fatherName,
        AcademicDegree? teacherDegree,
        AcademicTitle? teacherTitle,
        Post? teacherPost,
        string? email,
        string? telephone)
    {
        if (string.IsNullOrWhiteSpace(firstName))
        {
            return Result<ValidatedTeacherData>.Failure("Имя преподавателя не может быть пустым");
        }

        if (string.IsNullOrWhiteSpace(lastName))
        {
            return Result<ValidatedTeacherData>.Failure("Фамилия преподавателя не может быть пустой");
        }

        string trimmedFirstName = firstName.Trim();
        string trimmedLastName = lastName.Trim();
        string? trimmedFatherName = NormalizeOptionalText(fatherName);

        if (trimmedFirstName.Length < 2)
        {
            return Result<ValidatedTeacherData>.Failure("Длина имени должна быть от 2 до 50 символов");
        }

        if (trimmedFirstName.Length > 50)
        {
            return Result<ValidatedTeacherData>.Failure("Длина имени должна быть от 2 до 50 символов");
        }

        if (trimmedLastName.Length < 2)
        {
            return Result<ValidatedTeacherData>.Failure("Длина фамилии должна быть от 2 до 50 символов");
        }

        if (trimmedLastName.Length > 50)
        {
            return Result<ValidatedTeacherData>.Failure("Длина фамилии должна быть от 2 до 50 символов");
        }

        if (!string.IsNullOrWhiteSpace(trimmedFatherName))
        {
            if (trimmedFatherName.Length < 2)
            {
                return Result<ValidatedTeacherData>.Failure("Длина отчества должна быть от 2 до 50 символов");
            }

            if (trimmedFatherName.Length > 50)
            {
                return Result<ValidatedTeacherData>.Failure("Длина отчества должна быть от 2 до 50 символов");
            }
        }

        if (!IsValidName(trimmedFirstName))
        {
            return Result<ValidatedTeacherData>.Failure("Имя содержит недопустимые символы");
        }

        if (!IsValidName(trimmedLastName))
        {
            return Result<ValidatedTeacherData>.Failure("Фамилия содержит недопустимые символы");
        }

        if (!string.IsNullOrWhiteSpace(trimmedFatherName))
        {
            if (!IsValidName(trimmedFatherName))
            {
                return Result<ValidatedTeacherData>.Failure("Отчество содержит недопустимые символы");
            }
        }

        if (teacherDegree.HasValue)
        {
            if (!Enum.IsDefined(typeof(AcademicDegree), teacherDegree.Value))
            {
                return Result<ValidatedTeacherData>.Failure("Недопустимая ученая степень");
            }
        }

        if (teacherTitle.HasValue)
        {
            if (!Enum.IsDefined(typeof(AcademicTitle), teacherTitle.Value))
            {
                return Result<ValidatedTeacherData>.Failure("Недопустимое ученое звание");
            }
        }

        if (teacherPost.HasValue)
        {
            if (!Enum.IsDefined(typeof(Post), teacherPost.Value))
            {
                return Result<ValidatedTeacherData>.Failure("Недопустимая должность преподавателя");
            }
        }

        string? normalizedEmail = NormalizeOptionalText(email);

        if (!string.IsNullOrWhiteSpace(normalizedEmail))
        {
            if (ContainsDangerousCharacters(normalizedEmail))
            {
                return Result<ValidatedTeacherData>.Failure("Email содержит недопустимые символы");
            }

            if (!EmailRegex.IsMatch(normalizedEmail))
            {
                return Result<ValidatedTeacherData>.Failure("Некорректный формат Email");
            }
        }

        string? normalizedTelephone = NormalizePhone(telephone);

        if (!string.IsNullOrWhiteSpace(normalizedTelephone))
        {
            if (!PhoneRegex.IsMatch(normalizedTelephone))
            {
                return Result<ValidatedTeacherData>.Failure("Некорректный формат телефона");
            }
        }

        ValidatedTeacherData data = new ValidatedTeacherData
        {
            FirstName = trimmedFirstName,
            LastName = trimmedLastName,
            FatherName = trimmedFatherName,
            TeacherDegree = teacherDegree,
            TeacherTitle = teacherTitle,
            TeacherPost = teacherPost,
            Email = normalizedEmail,
            Telephone = normalizedTelephone
        };

        return Result<ValidatedTeacherData>.Success(data);
    }

    private static GetTeacherDto MapToGetTeacherDto(Teacher teacher)
    {
        GetTeacherDto dto = new GetTeacherDto
        {
            Id = teacher.Id,
            FullName = BuildFullName(teacher.LastName, teacher.FirstName, teacher.FatherName),
            TeacherPost = teacher.TeacherPost
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

    private static bool IsValidName(string value)
    {
        foreach (char symbol in value)
        {
            if (symbol == '-')
            {
                continue;
            }

            if (!char.IsLetter(symbol))
            {
                return false;
            }
        }

        return true;
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

        return false;
    }

    private static string? NormalizeOptionalText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }

    private static string? NormalizePhone(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        string trimmedValue = value.Trim();
        string normalizedValue = trimmedValue
            .Replace(" ", string.Empty)
            .Replace("-", string.Empty)
            .Replace("(", string.Empty)
            .Replace(")", string.Empty)
            .Replace(".", string.Empty);

        return normalizedValue;
    }

    private class ValidatedTeacherData
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string? FatherName { get; set; }
        public AcademicDegree? TeacherDegree { get; set; }
        public AcademicTitle? TeacherTitle { get; set; }
        public Post? TeacherPost { get; set; }
        public string? Email { get; set; }
        public string? Telephone { get; set; }
    }
}
