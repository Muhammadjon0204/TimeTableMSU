using Domain.Enums;

namespace Application.DTOs.PortalDTOs;

public class PortalAcademicPerformanceDto
{
    public int Id { get; set; }
    public string StudentFullName { get; set; } = null!;
    public string SubjectName { get; set; } = null!;
    public string TeacherFullName { get; set; } = null!;
    public ControlForm? ControlForm { get; set; }
    public short? Tur { get; set; }
    public short? Mark { get; set; }
}
