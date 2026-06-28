namespace Application.DTOs.AdminAcademicJournalDTOs;

public class StudentGradebookDto
{
    public int StudentId { get; set; }
    public string StudentFullName { get; set; } = string.Empty;
    public string? GroupName { get; set; }
    public string? SpecialityName { get; set; }
    public string? FacultyName { get; set; }
    public List<StudentSemesterGradebookDto> Semesters { get; set; } = new();
}

public class StudentSemesterGradebookDto
{
    public short Semester { get; set; }
    public List<StudentSubjectGradeDto> Subjects { get; set; } = new();
}

public class StudentSubjectGradeDto
{
    public int DisciplineId { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public string TeacherFullName { get; set; } = string.Empty;
    public string ControlForm { get; set; } = string.Empty;
    public string? Mark { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool HasRetake { get; set; }
    public int RetakeCount { get; set; }
}
