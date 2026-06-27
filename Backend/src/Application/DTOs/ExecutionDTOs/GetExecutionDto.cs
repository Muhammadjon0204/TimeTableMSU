namespace Application.DTOs.ExecutionDTOs;

public class GetExecutionDto
{
    public int Id { get; set; }
    public int ScheduleId { get; set; }
    public string ScheduleInfo { get; set; } = null!;
    public DateOnly ExecutionDate { get; set; }
    public string Status { get; set; } = null!;
    public string? Notes { get; set; }
}
