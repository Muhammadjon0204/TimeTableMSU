namespace Application.DTOs.PortalDTOs;

public class PortalStudentProfileDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? FatherName { get; set; }
    public string? Email { get; set; }
    public int? GroupId { get; set; }
    public string? GroupName { get; set; }
}
