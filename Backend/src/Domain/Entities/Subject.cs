using Domain.Enums;

namespace Domain.Entities;

public class Subject
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public short Semester { get; set; }
    public int HourCount { get; set; }
    public ControlForm ControlForm { get; set; }
    
    public ICollection<Discipline> Disciplines { get; set; } = new List<Discipline>();    
}
