namespace Domain.Entities;

public class Student
{
    public int Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? FatherName { get; set; }
    public int? GroupId { get; set; } 
    public string? Telephone { get; set; }
    public string? Address { get; set; }
    public string? Email { get; set; }
    public DateTime? BirthDate { get; set; }
    
    
    public Group? Group { get; set; } 
    public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
    public ICollection<AcademicPerformance> AcademicPerformances { get; set; } = new List<AcademicPerformance>();
}