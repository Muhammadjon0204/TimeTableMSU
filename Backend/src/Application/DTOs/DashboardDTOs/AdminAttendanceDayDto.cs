namespace Application.DTOs.DashboardDTOs;

public class AdminAttendanceDayDto
{
    public int Day { get; set; }
    public string DayName { get; set; } = string.Empty;
    public int Expected { get; set; }
    public int Present { get; set; }
    public int Late { get; set; }
    public int Absent { get; set; }
    public double AttendancePercent { get; set; }
}
