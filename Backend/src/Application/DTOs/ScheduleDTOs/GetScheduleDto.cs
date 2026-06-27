namespace Application.DTOs.ScheduleDTOs;

public class GetScheduleDto
{
    public int Id { get; set; }
    public int Den { get; set; }
    public int Para { get; set; }
    public string SubjectName { get; set; } = null!;
    public string TeacherFullName { get; set; } = null!;
    public string AudienceNumber { get; set; } = null!;
    public string GroupName { get; set; } = null!;
    public string LectureType { get; set; } = null!;
}
