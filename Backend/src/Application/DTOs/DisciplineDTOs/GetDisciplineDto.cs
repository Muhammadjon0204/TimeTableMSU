namespace Application.DTOs.DisciplineDTOs;

public class GetDisciplineDto
{
    public int Id { get; set; }
    public string SubjectName { get; set; } = null!;
    public string TeacherFullName { get; set; } = null!;
    public string GroupName { get; set; } = null!;
}
