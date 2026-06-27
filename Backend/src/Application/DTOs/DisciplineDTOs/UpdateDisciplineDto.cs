namespace Application.DTOs.DisciplineDTOs;

public class UpdateDisciplineDto
{
    public int Id { get; set; }
    public int SubjectId { get; set; }
    public int TeacherId { get; set; }
    public int GroupId { get; set; }
    public int? LectureHourCount { get; set; }
    public int? PracticeHourCount { get; set; }
    public int? LaboratoryHourCount { get; set; }
}
