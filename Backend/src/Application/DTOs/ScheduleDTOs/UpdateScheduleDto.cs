namespace Application.DTOs.ScheduleDTOs;

public class UpdateScheduleDto
{
    public int Id { get; set; }
    public int Den { get; set; }
    public int Para { get; set; }
    public int DisciplineId { get; set; }
    public int TeacherId { get; set; }
    public int AudienceId { get; set; }
    public int GroupId { get; set; }
    public int WeekId { get; set; }
    public string LectureType { get; set; } = null!;
}
