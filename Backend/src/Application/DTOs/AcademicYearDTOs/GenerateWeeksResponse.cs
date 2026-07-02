using Application.DTOs.WeekDTOs;

namespace Application.DTOs.AcademicYearDTOs;

public class GenerateWeeksResponse
{
    public int AcademicYearId { get; set; }
    public string AcademicYearName { get; set; } = string.Empty;
    public int CreatedCount { get; set; }
    public int SkippedCount { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly GenerateUntil { get; set; }
    public List<GetWeekDto> Weeks { get; set; } = new();
}
