namespace Application.DTOs.PortalDTOs;

public class PortalScheduleDto
{
    public int Id { get; set; }
    public int Den { get; set; }
    public int Para { get; set; }
    public string SubjectName { get; set; } = null!;
    public string TeacherFullName { get; set; } = null!;
    public string AudienceNumber { get; set; } = null!;
    public string GroupName { get; set; } = null!;
    public string? WeekName { get; set; }
    public string LectureType { get; set; } = null!;
}
