using Domain.Enums;

namespace Domain.Entities;

public class Weeks
{
    public int Id { get; set; }
    public int? AcademicYearId { get; set; }
    public int? AcademicPeriodId { get; set; }
    public string Name { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public WeekType Type { get; set; } = WeekType.Study;
    public bool IsCurrent { get; set; }

    public AcademicYear? AcademicYear { get; set; }
    public AcademicPeriod? AcademicPeriod { get; set; }
    public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
    public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
}
