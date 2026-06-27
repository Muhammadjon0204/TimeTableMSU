namespace Application.DTOs.WeekDTOs;

public class CreateWeekDto
{
    public string Name { get; set; } = null!;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
}
