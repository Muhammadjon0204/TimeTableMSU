using Domain.Enums;

namespace Application.DTOs.AdminAcademicJournalDTOs;

public class StudentJournalDto
{
    public StudentJournalHeaderDto Header { get; set; } = new();
    public List<StudentSemesterJournalDto> Semesters { get; set; } = new();
}

public class StudentJournalHeaderDto
{
    public int StudentId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;
    public string SpecialityName { get; set; } = string.Empty;
    public string FacultyName { get; set; } = string.Empty;
    public string? AcademicYear { get; set; }
}

public class StudentSemesterJournalDto
{
    public short Semester { get; set; }
    public string? AcademicYear { get; set; }
    public List<StudentJournalRowDto> Rows { get; set; } = new();
}

public class StudentJournalRowDto
{
    public int RowNumber { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public string ControlForm { get; set; } = string.Empty;
    public string TeacherFullName { get; set; } = string.Empty;
    public int? FirstControlWork { get; set; }
    public int? SecondControlWork { get; set; }
    public CreditResult? CreditResult { get; set; }
    public ExamResult? ExamResult { get; set; }
    public string FinalDisplayValue { get; set; } = string.Empty;
    public string StatusDisplayValue { get; set; } = string.Empty;
    public string RetakeStatusDisplayValue { get; set; } = string.Empty;
    public string RetakeRoundDisplayValue { get; set; } = string.Empty;
    public string? Note { get; set; }
    public string ResultType { get; set; } = string.Empty;
    public string RetakeType { get; set; } = string.Empty;
}
