using Domain.Enums;

namespace Domain.Entities;

public class AcademicPerformance
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int DisciplineId { get; set; } 
    public int TeacherId { get; set; }
    public ControlForm? ControlForm { get; set; }
    public short? Tur { get; set; }
    public short? Mark { get; set; }
    
    public Student Student { get; set; } = null!;
    public Discipline Discipline { get; set; } = null!;
    public Teacher Teacher { get; set; } = null!;
}