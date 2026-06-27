using System.Text.RegularExpressions;
using Application.Common;
using Application.DTOs.StudentDTOs;
using Application.Interfaces.Interface;
using Application.Interfaces.Repository;
using Domain.Entities;
using DomainGroup = Domain.Entities.Group;

namespace Application.Service;

public class StudentService : IStudentService
{
    private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);
    private static readonly Regex PhoneRegex = new(@"^\+?[1-9]\d{1,14}$", RegexOptions.Compiled);

    private readonly IStudentRepository _studentRepository;
    private readonly IGroupRepository _groupRepository;

    public StudentService(IStudentRepository studentRepository, IGroupRepository groupRepository)
    {
        _studentRepository = studentRepository;
        _groupRepository = groupRepository;
    }

    public async Task<Result<GetStudentDetailDto>> GetByIdAsync(int id)
    {
        if (id <= 0)
        {
            return Result<GetStudentDetailDto>.Failure("Некорректный идентификатор студента");
        }

        Student? student = await _studentRepository.GetByIdAsync(id);

        if (student == null)
        {
            return Result<GetStudentDetailDto>.Failure("Студент не найден");
        }

        string? groupName = student.Group?.Name;

        if (string.IsNullOrWhiteSpace(groupName))
        {
            if (student.GroupId != null)
            {
                DomainGroup? group = await _groupRepository.GetByIdAsync(student.GroupId.Value);

                if (group != null)
                {
                    groupName = group.Name;
                }
            }
        }

        GetStudentDetailDto mappedDto = MapToGetStudentDetailDto(student, groupName);

        return Result<GetStudentDetailDto>.Success(mappedDto);
    }

    public async Task<Result<PagedResult<GetStudentDto>>> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm)
    {
        if (pageNumber < 1)
        {
            pageNumber = 1;
        }

        if (pageSize < 1)
        {
            pageSize = 10;
        }

        PagedResult<Student> students = await _studentRepository.GetPagedAsync(pageNumber, pageSize, searchTerm);

        List<GetStudentDto> mappedDtos = new List<GetStudentDto>();

        foreach (Student student in students.Items)
        {
            string? groupName = student.Group?.Name;

            if (string.IsNullOrWhiteSpace(groupName))
            {
                if (student.GroupId != null)
                {
                    DomainGroup? group = await _groupRepository.GetByIdAsync(student.GroupId.Value);

                    if (group != null)
                    {
                        groupName = group.Name;
                    }
                }
            }

            GetStudentDto mappedDto = MapToGetStudentDto(student, groupName);
            mappedDtos.Add(mappedDto);
        }

        PagedResult<GetStudentDto> result = new PagedResult<GetStudentDto>(
            mappedDtos,
            students.TotalCount,
            students.PageNumber,
            students.PageSize);

        return Result<PagedResult<GetStudentDto>>.Success(result);
    }

    public async Task<Result<GetStudentDto>> CreateAsync(CreateStudentDto dto)
    {
        if (dto == null)
        {
            return Result<GetStudentDto>.Failure("Данные запроса отсутствуют");
        }

        Result<ValidatedStudentData> validationResult = ValidateStudentData(
            dto.FirstName,
            dto.LastName,
            dto.FatherName,
            dto.Email,
            dto.Telephone,
            dto.BirthDate,
            dto.GroupId);

        if (validationResult.IsFailure)
        {
            return Result<GetStudentDto>.Failure(validationResult.Error);
        }

        ValidatedStudentData validatedData = validationResult.Value;

        DomainGroup? group = await _groupRepository.GetByIdAsync(validatedData.GroupId);

        if (group == null)
        {
            return Result<GetStudentDto>.Failure("Указанная академическая группа не существует");
        }

        List<Student> students = await _studentRepository.GetAllAsync();

        foreach (Student existingStudent in students)
        {
            if (!string.IsNullOrWhiteSpace(validatedData.Email))
            {
                if (!string.IsNullOrWhiteSpace(existingStudent.Email))
                {
                    if (string.Equals(existingStudent.Email.Trim(), validatedData.Email, StringComparison.OrdinalIgnoreCase))
                    {
                        return Result<GetStudentDto>.Failure("Студент с таким Email уже существует");
                    }
                }
            }

            bool sameFirstName = string.Equals(existingStudent.FirstName.Trim(), validatedData.FirstName, StringComparison.OrdinalIgnoreCase);
            bool sameLastName = string.Equals(existingStudent.LastName.Trim(), validatedData.LastName, StringComparison.OrdinalIgnoreCase);
            bool sameFatherName = string.Equals((existingStudent.FatherName ?? string.Empty).Trim(), validatedData.FatherName ?? string.Empty, StringComparison.OrdinalIgnoreCase);
            bool sameBirthDate = existingStudent.BirthDate.HasValue && DateOnly.FromDateTime(existingStudent.BirthDate.Value) == validatedData.BirthDate;

            if (sameFirstName && sameLastName && sameFatherName && sameBirthDate)
            {
                return Result<GetStudentDto>.Failure("Студент с такими ФИО и датой рождения уже существует");
            }
        }

        Student student = new Student
        {
            FirstName = validatedData.FirstName,
            LastName = validatedData.LastName,
            FatherName = validatedData.FatherName,
            GroupId = validatedData.GroupId,
            Group = group,
            Telephone = validatedData.Telephone,
            Address = NormalizeOptionalText(dto.Address),
            Email = validatedData.Email,
            BirthDate = validatedData.BirthDate.ToDateTime(TimeOnly.MinValue)
        };

        await _studentRepository.AddAsync(student);

        GetStudentDto mappedDto = MapToGetStudentDto(student, group.Name);

        return Result<GetStudentDto>.Success(mappedDto);
    }

    public async Task<Result<GetStudentDto>> UpdateAsync(UpdateStudentDto dto)
    {
        if (dto == null)
        {
            return Result<GetStudentDto>.Failure("Данные запроса отсутствуют");
        }

        if (dto.Id <= 0)
        {
            return Result<GetStudentDto>.Failure("Некорректный идентификатор студента");
        }

        if (dto.GroupId <= 0)
        {
            return Result<GetStudentDto>.Failure("Некорректный идентификатор группы");
        }

        Result<ValidatedStudentData> validationResult = ValidateStudentData(
            dto.FirstName,
            dto.LastName,
            dto.FatherName,
            dto.Email,
            dto.Telephone,
            dto.BirthDate,
            dto.GroupId);

        if (validationResult.IsFailure)
        {
            return Result<GetStudentDto>.Failure(validationResult.Error);
        }

        ValidatedStudentData validatedData = validationResult.Value;

        Student? student = await _studentRepository.GetByIdAsync(dto.Id);

        if (student == null)
        {
            return Result<GetStudentDto>.Failure("Студент не найден");
        }

        DomainGroup? group = await _groupRepository.GetByIdAsync(validatedData.GroupId);

        if (group == null)
        {
            return Result<GetStudentDto>.Failure("Указанная академическая группа не существует");
        }

        List<Student> students = await _studentRepository.GetAllAsync();

        foreach (Student existingStudent in students)
        {
            if (existingStudent.Id != dto.Id)
            {
                if (!string.IsNullOrWhiteSpace(validatedData.Email))
                {
                    if (!string.IsNullOrWhiteSpace(existingStudent.Email))
                    {
                        if (string.Equals(existingStudent.Email.Trim(), validatedData.Email, StringComparison.OrdinalIgnoreCase))
                        {
                            return Result<GetStudentDto>.Failure("Email уже используется другим студентом");
                        }
                    }
                }

                bool sameFirstName = string.Equals(existingStudent.FirstName.Trim(), validatedData.FirstName, StringComparison.OrdinalIgnoreCase);
                bool sameLastName = string.Equals(existingStudent.LastName.Trim(), validatedData.LastName, StringComparison.OrdinalIgnoreCase);
                bool sameFatherName = string.Equals((existingStudent.FatherName ?? string.Empty).Trim(), validatedData.FatherName ?? string.Empty, StringComparison.OrdinalIgnoreCase);
                bool sameBirthDate = existingStudent.BirthDate.HasValue && DateOnly.FromDateTime(existingStudent.BirthDate.Value) == validatedData.BirthDate;

                if (sameFirstName && sameLastName && sameFatherName && sameBirthDate)
                {
                    return Result<GetStudentDto>.Failure("Другой студент с такими ФИО и датой рождения уже существует");
                }
            }
        }

        student.FirstName = validatedData.FirstName;
        student.LastName = validatedData.LastName;
        student.FatherName = validatedData.FatherName;
        student.GroupId = validatedData.GroupId;
        student.Group = group;
        student.Telephone = validatedData.Telephone;
        student.Address = NormalizeOptionalText(dto.Address);
        student.Email = validatedData.Email;
        student.BirthDate = validatedData.BirthDate.ToDateTime(TimeOnly.MinValue);

        await _studentRepository.UpdateAsync(student);

        GetStudentDto mappedDto = MapToGetStudentDto(student, group.Name);

        return Result<GetStudentDto>.Success(mappedDto);
    }

    public async Task<Result> DeleteAsync(int id)
    {
        if (id <= 0)
        {
            return Result.Failure("Некорректный идентификатор студента");
        }

        Student? student = await _studentRepository.GetByIdAsync(id);

        if (student == null)
        {
            return Result.Failure("Студент для удаления не найден");
        }

        await _studentRepository.DeleteAsync(student);

        return Result.Success();
    }

    private static Result<ValidatedStudentData> ValidateStudentData(
        string firstName,
        string lastName,
        string? fatherName,
        string? email,
        string? telephone,
        DateOnly? birthDate,
        int? groupId)
    {
        if (string.IsNullOrWhiteSpace(firstName))
        {
            return Result<ValidatedStudentData>.Failure("Имя студента не может быть пустым");
        }

        if (string.IsNullOrWhiteSpace(lastName))
        {
            return Result<ValidatedStudentData>.Failure("Фамилия студента не может быть пустой");
        }

        string trimmedFirstName = firstName.Trim();
        string trimmedLastName = lastName.Trim();
        string? trimmedFatherName = NormalizeOptionalText(fatherName);

        if (trimmedFirstName.Length < 2)
        {
            return Result<ValidatedStudentData>.Failure("Длина имени должна быть от 2 до 50 символов");
        }

        if (trimmedFirstName.Length > 50)
        {
            return Result<ValidatedStudentData>.Failure("Длина имени должна быть от 2 до 50 символов");
        }

        if (trimmedLastName.Length < 2)
        {
            return Result<ValidatedStudentData>.Failure("Длина фамилии должна быть от 2 до 50 символов");
        }

        if (trimmedLastName.Length > 50)
        {
            return Result<ValidatedStudentData>.Failure("Длина фамилии должна быть от 2 до 50 символов");
        }

        if (!string.IsNullOrWhiteSpace(trimmedFatherName))
        {
            if (trimmedFatherName.Length < 2)
            {
                return Result<ValidatedStudentData>.Failure("Длина отчества должна быть от 2 до 50 символов");
            }

            if (trimmedFatherName.Length > 50)
            {
                return Result<ValidatedStudentData>.Failure("Длина отчества должна быть от 2 до 50 символов");
            }
        }

        if (!IsValidName(trimmedFirstName))
        {
            return Result<ValidatedStudentData>.Failure("Имя содержит недопустимые символы");
        }

        if (!IsValidName(trimmedLastName))
        {
            return Result<ValidatedStudentData>.Failure("Фамилия содержит недопустимые символы");
        }

        if (!string.IsNullOrWhiteSpace(trimmedFatherName))
        {
            if (!IsValidName(trimmedFatherName))
            {
                return Result<ValidatedStudentData>.Failure("Отчество содержит недопустимые символы");
            }
        }

        string? normalizedEmail = NormalizeOptionalText(email);

        if (!string.IsNullOrWhiteSpace(normalizedEmail))
        {
            if (!EmailRegex.IsMatch(normalizedEmail))
            {
                return Result<ValidatedStudentData>.Failure("Некорректный формат Email");
            }
        }

        string? normalizedTelephone = NormalizePhone(telephone);

        if (!string.IsNullOrWhiteSpace(normalizedTelephone))
        {
            if (!PhoneRegex.IsMatch(normalizedTelephone))
            {
                return Result<ValidatedStudentData>.Failure("Некорректный формат телефона");
            }
        }

        if (birthDate == null)
        {
            return Result<ValidatedStudentData>.Failure("Дата рождения обязательна");
        }

        DateOnly today = DateOnly.FromDateTime(DateTime.Today);

        if (birthDate.Value > today)
        {
            return Result<ValidatedStudentData>.Failure("Дата рождения не может быть из будущего");
        }

        int age = CalculateAge(birthDate.Value, today);

        if (age < 14)
        {
            return Result<ValidatedStudentData>.Failure("Студент не может быть младше 14 лет");
        }

        if (age > 100)
        {
            return Result<ValidatedStudentData>.Failure("Студент не может быть старше 100 лет");
        }

        if (groupId == null)
        {
            return Result<ValidatedStudentData>.Failure("Некорректный идентификатор группы");
        }

        if (groupId <= 0)
        {
            return Result<ValidatedStudentData>.Failure("Некорректный идентификатор группы");
        }

        ValidatedStudentData data = new ValidatedStudentData
        {
            FirstName = trimmedFirstName,
            LastName = trimmedLastName,
            FatherName = trimmedFatherName,
            Email = normalizedEmail,
            Telephone = normalizedTelephone,
            BirthDate = birthDate.Value,
            GroupId = groupId.Value
        };

        return Result<ValidatedStudentData>.Success(data);
    }

    private static GetStudentDto MapToGetStudentDto(Student student, string? groupName)
    {
        GetStudentDto dto = new GetStudentDto
        {
            Id = student.Id,
            FullName = BuildFullName(student.LastName, student.FirstName, student.FatherName),
            FirstName = student.FirstName,
            LastName = student.LastName,
            FatherName = student.FatherName,
            GroupName = groupName,
            Email = student.Email
        };

        return dto;
    }

    private static GetStudentDetailDto MapToGetStudentDetailDto(Student student, string? groupName)
    {
        DateOnly? birthDate = null;

        if (student.BirthDate.HasValue)
        {
            birthDate = DateOnly.FromDateTime(student.BirthDate.Value);
        }

        GetStudentDetailDto dto = new GetStudentDetailDto
        {
            Id = student.Id,
            FirstName = student.FirstName,
            LastName = student.LastName,
            FatherName = student.FatherName,
            GroupId = student.GroupId,
            GroupName = groupName,
            Telephone = student.Telephone,
            Address = student.Address,
            Email = student.Email,
            BirthDate = birthDate
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

    private static int CalculateAge(DateOnly birthDate, DateOnly today)
    {
        int age = today.Year - birthDate.Year;

        if (today.Month < birthDate.Month)
        {
            age--;
        }
        else if (today.Month == birthDate.Month)
        {
            if (today.Day < birthDate.Day)
            {
                age--;
            }
        }

        return age;
    }

    private class ValidatedStudentData
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string? FatherName { get; set; }
        public string? Email { get; set; }
        public string? Telephone { get; set; }
        public DateOnly BirthDate { get; set; }
        public int GroupId { get; set; }
    }
}
