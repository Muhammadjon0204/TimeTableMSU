using Domain.Enums;

namespace Application.DTOs.SubjectDTOs;

public class CreateSubjectDto
{
    public string Name { get; set; } = null!;
    public short? Semester { get; set; }
    public int? HourCount { get; set; }
    public ControlForm ControlForm { get; set; }
}
