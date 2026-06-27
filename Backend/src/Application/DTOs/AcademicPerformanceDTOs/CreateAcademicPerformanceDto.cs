namespace Application.DTOs.AcademicPerformanceDTOs;

public class CreateAcademicPerformanceDto
{
    public int StudentId { get; set; }
    public int DisciplineId { get; set; }
    public int TeacherId { get; set; }
    public short? Mark { get; set; }
}
