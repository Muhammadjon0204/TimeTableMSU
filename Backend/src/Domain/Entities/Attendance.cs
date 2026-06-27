using Domain.Enums;

namespace Domain.Entities;

public class Attendance
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int WeekId { get; set; }
    public short Day { get; set; }
    public short Para { get; set; }
    public AttendanceMark? Mark { get; set; }
    
    
    public Student Student { get; set; } = null!;
    public Weeks Week { get; set; } = null!;
}