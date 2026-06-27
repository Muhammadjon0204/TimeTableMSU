using Domain.Enums;

namespace Application.DTOs.AttendanceDTOs;

public class GetAttendanceDto
{
    public int Id { get; set; }
    public short Day { get; set; }
    public short Para { get; set; }
    public AttendanceMark? Mark { get; set; }
    public string StudentFullName { get; set; } = null!;
    public string? WeekName { get; set; }
}
