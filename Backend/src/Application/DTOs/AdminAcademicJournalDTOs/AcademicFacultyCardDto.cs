namespace Application.DTOs.AdminAcademicJournalDTOs;

public class AcademicFacultyCardDto
{
    public int FacultyId { get; set; }
    public string FacultyName { get; set; } = string.Empty;
    public int SpecialitiesCount { get; set; }
    public int GroupsCount { get; set; }
    public int StudentsCount { get; set; }
}
