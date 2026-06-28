namespace Application.DTOs.AdminAcademicJournalDTOs;

public class AcademicStudentCardDto
{
    public int StudentId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Telephone { get; set; }
    public string? Address { get; set; }
    public int MarksCount { get; set; }
    public double? AverageMark { get; set; }
}
