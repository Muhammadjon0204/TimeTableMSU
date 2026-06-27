namespace Domain.Entities;

public class Weeks
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    
    public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
    public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
}
