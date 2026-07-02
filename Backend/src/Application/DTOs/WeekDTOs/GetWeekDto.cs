namespace Application.DTOs.WeekDTOs;

public class GetWeekDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public int? AcademicYearId { get; set; }
    public string AcademicYearName { get; set; } = string.Empty;
    public string WeekType { get; set; } = string.Empty;
    public bool IsCurrent { get; set; }
}
