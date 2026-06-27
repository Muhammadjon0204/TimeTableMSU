namespace Application.DTOs.ExecutionDTOs;

public class CreateExecutionDto
{
    public int ScheduleId { get; set; }
    public DateOnly ExecutionDate { get; set; }
    public string Status { get; set; } = null!;
    public string? Notes { get; set; }
}
