namespace Application.DTOs.AdminAcademicJournalDTOs;

public class AcademicGroupCardDto
{
    public int GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public short Course { get; set; }
    public string SpecialityName { get; set; } = string.Empty;
    public int StudentsCount { get; set; }
    public int MarksCount { get; set; }
}
