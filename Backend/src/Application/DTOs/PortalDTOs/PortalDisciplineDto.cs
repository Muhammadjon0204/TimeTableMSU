namespace Application.DTOs.PortalDTOs;

public class PortalDisciplineDto
{
    public int Id { get; set; }
    public string SubjectName { get; set; } = null!;
    public string TeacherFullName { get; set; } = null!;
    public string GroupName { get; set; } = null!;
    public int? LectureHourCount { get; set; }
    public int? PracticeHourCount { get; set; }
    public int? LaboratoryHourCount { get; set; }
    public int? OtherHourCount { get; set; }
}
