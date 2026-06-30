namespace Domain.Entities;

public class Holiday
{
    public int Id { get; set; }
    public int AcademicYearId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public bool IsRecurringByDate { get; set; }
    public bool IsStudyBlocked { get; set; }
    public string? Note { get; set; }

    public AcademicYear AcademicYear { get; set; } = null!;
}
