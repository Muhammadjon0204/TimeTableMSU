namespace Application.DTOs.StudentDTOs;

public class CreateStudentDto
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? FatherName { get; set; }
    public int? GroupId { get; set; } 
    public string? Telephone { get; set; }
    public string? Address { get; set; }
    public string? Email { get; set; }
    public DateOnly? BirthDate { get; set; }
}