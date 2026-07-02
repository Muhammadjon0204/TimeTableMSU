namespace Application.DTOs.AcademicYearDTOs;

public class AcademicYearDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public bool IsCurrent { get; set; }
    public int WeeksCount { get; set; }
}
