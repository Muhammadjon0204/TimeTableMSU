namespace Domain.Entities;

public class AcademicYear
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsCurrent { get; set; }
    public DateTime CreatedAt { get; set; }

    public ICollection<AcademicPeriod> Periods { get; set; } = new List<AcademicPeriod>();
    public ICollection<Weeks> Weeks { get; set; } = new List<Weeks>();
}
