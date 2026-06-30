using Domain.Enums;

namespace Domain.Entities;

public class AcademicPeriod
{
    public int Id { get; set; }
    public int AcademicYearId { get; set; }
    public string Name { get; set; } = string.Empty;
    public AcademicPeriodType Type { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int? Semester { get; set; }
    public bool IsActive { get; set; }

    public AcademicYear AcademicYear { get; set; } = null!;
    public ICollection<Weeks> Weeks { get; set; } = new List<Weeks>();
}
