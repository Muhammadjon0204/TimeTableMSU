namespace Domain.Entities;

public class Discipline
{
    public int Id { get; set; } 
    public int SubjectId { get; set; }
    public int TeacherId { get; set; }
    public int GroupId { get; set; }
    
    public int? LectureHourCount { get; set; }
    public int? PracticeHourCount { get; set; }
    public int? LaboratoryHourCount { get; set; }
    public int? OtherHourCount { get; set; }
    public int? Control { get; set; }
    
    public Subject Subject { get; set; } = null!;
    public Teacher Teacher { get; set; } = null!;
    public Group Group { get; set; } = null!;

    public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
    public ICollection<AcademicPerformance> AcademicPerformances { get; set; } = new List<AcademicPerformance>();
}
