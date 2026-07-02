namespace Application.DTOs.AcademicYearDTOs;

public class GenerateWeeksRequest
{
    public DateOnly? StartDate { get; set; }
    public DateOnly? GenerateUntil { get; set; }
    public bool SkipExistingWeeks { get; set; } = true;
    public bool OverwriteExistingWeeks { get; set; }
}
