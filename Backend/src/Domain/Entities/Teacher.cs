using Domain.Enums;

namespace Domain.Entities;

public class Teacher
{
    public int Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? FatherName { get; set; }
    public AcademicDegree? TeacherDegree { get; set; }
    public AcademicTitle? TeacherTitle { get; set; }
    public Post? TeacherPost { get; set; }
    public string? Telephone { get; set; }
    public string? Address { get; set; }
    public string? Email { get; set; }
    
    
    // Навигационные свойства
    public ICollection<Discipline> Disciplines { get; set; } = new List<Discipline>();
    public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
    public ICollection<AcademicPerformance> AcademicPerformances { get; set; } = new List<AcademicPerformance>();
}
