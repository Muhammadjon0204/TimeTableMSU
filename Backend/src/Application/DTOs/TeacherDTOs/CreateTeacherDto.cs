using Domain.Enums;

namespace Application.DTOs.TeacherDTOs;

public class CreateTeacherDto
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
