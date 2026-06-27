using Domain.Enums;

namespace Application.DTOs.AttendanceDTOs;

public class UpdateAttendanceDto
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int WeekId { get; set; }
    public short Day { get; set; }
    public short Para { get; set; }
    public AttendanceMark? Mark { get; set; }
}
