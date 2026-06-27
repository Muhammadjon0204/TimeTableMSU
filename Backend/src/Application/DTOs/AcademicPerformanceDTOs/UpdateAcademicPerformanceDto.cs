namespace Application.DTOs.AcademicPerformanceDTOs;

public class UpdateAcademicPerformanceDto
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int DisciplineId { get; set; }
    public int TeacherId { get; set; }
    public short? Mark { get; set; }
}
