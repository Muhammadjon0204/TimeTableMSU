namespace Application.DTOs.ScheduleBoardDTOs;

public class LookupDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class WeekLookupDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string AcademicYearName { get; set; } = string.Empty;
    public string PeriodName { get; set; } = string.Empty;
    public string PeriodType { get; set; } = string.Empty;
    public string WeekType { get; set; } = string.Empty;
    public bool IsCurrent { get; set; }
}
