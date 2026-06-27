namespace Application.DTOs.DashboardDTOs;

public class AdminAttendanceWeeklyDto
{
    public int? WeekId { get; set; }
    public string? WeekName { get; set; }
    public int? GroupId { get; set; }
    public string GroupName { get; set; } = "Все группы";
    public List<AdminAttendanceDayDto> Days { get; set; } = new();
}
