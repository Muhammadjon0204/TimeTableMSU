using Domain.Enums;

namespace Application.DTOs.PortalDTOs;

public class PortalTeacherProfileDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? FatherName { get; set; }
    public string? Email { get; set; }
    public AcademicDegree? TeacherDegree { get; set; }
    public AcademicTitle? TeacherTitle { get; set; }
    public Post? TeacherPost { get; set; }
}
