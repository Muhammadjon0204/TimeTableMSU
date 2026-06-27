namespace Application.DTOs.AcademicPerformanceDTOs;

public class GetAcademicPerformanceDto
{
    public int Id { get; set; }
    public string StudentFullName { get; set; } = null!;
    public string SubjectName { get; set; } = null!;
    public short? Mark { get; set; }
}
