namespace Domain.Entities;

public class Execution
{
    public int Id { get; set; }
    public int ScheduleId { get; set; }
    public DateOnly ExecutionDate { get; set; }
    public string Status { get; set; } = null!;
    public string? Notes { get; set; }
    
    // Навигационные свойства
    public Schedule Schedule { get; set; } = null!;
}
