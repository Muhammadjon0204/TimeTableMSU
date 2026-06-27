using Domain.Enums;

namespace Application.DTOs.SubjectDTOs;

public class GetSubjectDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public ControlForm ControlForm { get; set; }
}
