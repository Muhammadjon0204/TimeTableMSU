namespace Application.DTOs.DisciplineDTOs;

public class GetDisciplineDto
{
    public int Id { get; set; }
    public int SubjectId { get; set; }
    public string SubjectName { get; set; } = null!;
    public int TeacherId { get; set; }
    public string TeacherFullName { get; set; } = null!;
    public int GroupId { get; set; }
    public string GroupName { get; set; } = null!;
    public int? LectureHourCount { get; set; }
    public int? PracticeHourCount { get; set; }
    public int? LaboratoryHourCount { get; set; }
}
