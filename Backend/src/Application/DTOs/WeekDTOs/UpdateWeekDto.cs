namespace Application.DTOs.WeekDTOs;

public class UpdateWeekDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public int? AcademicYearId { get; set; }
}
