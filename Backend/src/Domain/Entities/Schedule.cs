using Domain.Enums;

namespace Domain.Entities;

public class Schedule
{
    public int Id { get; set; }
    public short Den { get; set; }
    public short Para { get; set; }
    public int DisciplineId { get; set; } 
    public int TeacherId { get; set; }
    public LectureType? LectureType { get; set; }
    public int AudienceId { get; set; }
    public int GroupId { get; set; }
    public int WeekId { get; set; }
    
    // Навигационные свойства
    public Discipline Discipline { get; set; } = null!;
    public Teacher Teacher { get; set; } = null!;
    public Audience Audience { get; set; } = null!;
    public Group Group { get; set; } = null!;
    public Weeks Week { get; set; } = null!;
    public ICollection<Execution> Executions { get; set; } = new List<Execution>();
}
