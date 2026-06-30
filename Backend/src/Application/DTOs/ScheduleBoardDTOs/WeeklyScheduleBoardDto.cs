namespace Application.DTOs.ScheduleBoardDTOs;

public class WeeklyScheduleBoardDto
{
    public int? WeekId { get; set; }
    public string WeekName { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string AcademicYearName { get; set; } = string.Empty;
    public string PeriodName { get; set; } = string.Empty;
    public string PeriodType { get; set; } = string.Empty;
    public string WeekType { get; set; } = string.Empty;
    public bool IsCurrentWeek { get; set; }
    public bool IsVacation { get; set; }
    public bool IsPractice { get; set; }
    public bool IsExamSession { get; set; }
    public List<ScheduleDayDto> Days { get; set; } = new();
}

public class ScheduleDayDto
{
    public int Day { get; set; }
    public string DayName { get; set; } = string.Empty;
    public DateTime? Date { get; set; }
    public bool IsHoliday { get; set; }
    public string? HolidayName { get; set; }
    public List<ScheduleParaDto> Paras { get; set; } = new();
}

public class ScheduleParaDto
{
    public int Para { get; set; }
    public string TimeRange { get; set; } = string.Empty;
    public List<ScheduleLessonDto> Lessons { get; set; } = new();
}

public class ScheduleLessonDto
{
    public int ScheduleId { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public string TeacherFullName { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;
    public string AudienceNumber { get; set; } = string.Empty;
    public string LectureType { get; set; } = string.Empty;
    public string? AudienceType { get; set; }
    public int Day { get; set; }
    public int Para { get; set; }
}
