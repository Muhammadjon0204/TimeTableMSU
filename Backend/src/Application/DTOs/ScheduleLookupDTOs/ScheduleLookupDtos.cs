namespace Application.DTOs.ScheduleLookupDTOs;

public class LookupDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class WeekLookupDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string AcademicYearName { get; set; } = string.Empty;
    public string PeriodName { get; set; } = string.Empty;
    public string PeriodType { get; set; } = string.Empty;
    public string WeekType { get; set; } = string.Empty;
    public bool IsCurrent { get; set; }
}

public class AcademicPeriodLookupDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string AcademicYearName { get; set; } = string.Empty;
}

public class HolidayLookupDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public bool IsStudyBlocked { get; set; }
    public string? Note { get; set; }
}

public class SubjectLookupDto
{
    public int SubjectId { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public int Semester { get; set; }
    public string ControlForm { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}

public class DisciplineScheduleOptionDto
{
    public int DisciplineId { get; set; }
    public int SubjectId { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public int TeacherId { get; set; }
    public string TeacherFullName { get; set; } = string.Empty;
    public int GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public int SpecialityId { get; set; }
    public string SpecialityName { get; set; } = string.Empty;
    public int FacultyId { get; set; }
    public string FacultyName { get; set; } = string.Empty;
    public int Semester { get; set; }
}

public class AudienceLookupDto
{
    public int Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}
