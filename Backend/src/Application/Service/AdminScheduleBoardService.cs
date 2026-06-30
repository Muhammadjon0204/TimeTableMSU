using Application.Common;
using Application.DTOs.ScheduleBoardDTOs;
using Application.Interfaces.Interface;
using Application.Interfaces.Repository;
using Domain.Entities;
using Domain.Enums;

namespace Application.Service;

public class AdminScheduleBoardService : IAdminScheduleBoardService
{
    private readonly IAdminScheduleBoardRepository _repository;

    public AdminScheduleBoardService(IAdminScheduleBoardRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<WeeklyScheduleBoardDto>> GetWeeklyAsync(int? weekId, int? groupId, int? teacherId, int? audienceId)
    {
        List<Weeks> weeks = await _repository.GetWeeksAsync();

        if (weeks.Count == 0)
        {
            return Result<WeeklyScheduleBoardDto>.Success(CreateEmptyBoard(null));
        }

        Weeks? selectedWeek = SelectWeek(weeks, weekId);

        if (selectedWeek == null)
        {
            return Result<WeeklyScheduleBoardDto>.Failure("Учебная неделя не найдена");
        }

        List<Schedule> schedules = await _repository.GetWeeklySchedulesAsync(selectedWeek.Id, groupId, teacherId, audienceId);
        List<Holiday> holidays = await _repository.GetHolidaysAsync(selectedWeek.StartDate, selectedWeek.EndDate);
        WeeklyScheduleBoardDto board = MapBoard(selectedWeek, schedules, holidays);

        return Result<WeeklyScheduleBoardDto>.Success(board);
    }

    public async Task<Result<WeeklyScheduleBoardDto>> GetCurrentWeekAsync(int? groupId, int? teacherId, int? audienceId)
    {
        return await GetWeeklyAsync(null, groupId, teacherId, audienceId);
    }

    public async Task<Result<List<WeekLookupDto>>> GetWeekLookupsAsync()
    {
        List<Weeks> weeks = await _repository.GetWeeksAsync();
        List<WeekLookupDto> result = weeks.Select(week => new WeekLookupDto
        {
            Id = week.Id,
            Name = week.Name,
            StartDate = week.StartDate,
            EndDate = week.EndDate,
            DisplayName = BuildWeekDisplayName(week),
            AcademicYearName = week.AcademicYear?.Name ?? string.Empty,
            PeriodName = week.AcademicPeriod?.Name ?? string.Empty,
            PeriodType = week.AcademicPeriod?.Type.ToString() ?? string.Empty,
            WeekType = week.Type.ToString(),
            IsCurrent = week.IsCurrent || IsWithin(DateTime.Today, week)
        }).ToList();

        return Result<List<WeekLookupDto>>.Success(result);
    }

    public async Task<Result<List<LookupDto>>> GetGroupLookupsAsync()
    {
        List<Group> groups = await _repository.GetGroupsAsync();
        List<LookupDto> result = groups.Select(group => new LookupDto
        {
            Id = group.Id,
            Name = group.Name
        }).ToList();

        return Result<List<LookupDto>>.Success(result);
    }

    public async Task<Result<List<LookupDto>>> GetTeacherLookupsAsync()
    {
        List<Teacher> teachers = await _repository.GetTeachersAsync();
        List<LookupDto> result = teachers.Select(teacher => new LookupDto
        {
            Id = teacher.Id,
            Name = BuildFullName(teacher.LastName, teacher.FirstName, teacher.FatherName)
        }).ToList();

        return Result<List<LookupDto>>.Success(result);
    }

    public async Task<Result<List<LookupDto>>> GetAudienceLookupsAsync()
    {
        List<Audience> audiences = await _repository.GetAudiencesAsync();
        List<LookupDto> result = audiences.Select(audience => new LookupDto
        {
            Id = audience.Id,
            Name = audience.Number
        }).ToList();

        return Result<List<LookupDto>>.Success(result);
    }

    private static Weeks? SelectWeek(List<Weeks> weeks, int? weekId)
    {
        if (weekId.HasValue)
        {
            return weeks.FirstOrDefault(week => week.Id == weekId.Value);
        }

        DateTime today = DateTime.Today;
        Weeks? currentWeek = weeks.FirstOrDefault(week => IsWithin(today, week));

        if (currentWeek != null)
        {
            return currentWeek;
        }

        Weeks? nextWeek = weeks.FirstOrDefault(week => week.StartDate.Date > today);

        return nextWeek ?? weeks.LastOrDefault();
    }

    private static WeeklyScheduleBoardDto MapBoard(Weeks week, List<Schedule> schedules, List<Holiday> holidays)
    {
        var holidaysByDate = holidays
            .GroupBy(holiday => holiday.Date.Date)
            .ToDictionary(group => group.Key, group => string.Join(", ", group.Select(holiday => holiday.Name)));

        WeeklyScheduleBoardDto board = new WeeklyScheduleBoardDto
        {
            WeekId = week.Id,
            WeekName = week.Name,
            StartDate = week.StartDate,
            EndDate = week.EndDate,
            AcademicYearName = week.AcademicYear?.Name ?? string.Empty,
            PeriodName = week.AcademicPeriod?.Name ?? string.Empty,
            PeriodType = week.AcademicPeriod?.Type.ToString() ?? string.Empty,
            WeekType = week.Type.ToString(),
            IsCurrentWeek = week.IsCurrent || IsWithin(DateTime.Today, week),
            IsVacation = week.Type == WeekType.Vacation || week.AcademicPeriod?.Type is AcademicPeriodType.WinterVacation or AcademicPeriodType.SpringVacation or AcademicPeriodType.SummerVacation,
            IsPractice = week.Type == WeekType.Practice || week.AcademicPeriod?.Type == AcademicPeriodType.SummerPractice,
            IsExamSession = week.Type == WeekType.Exam || week.AcademicPeriod?.Type == AcademicPeriodType.ExamSession
        };

        for (int day = 1; day <= 6; day++)
        {
            DateTime dayDate = week.StartDate.Date.AddDays(day - 1);
            List<Schedule> daySchedules = schedules
                .Where(schedule => schedule.Den == day)
                .OrderBy(schedule => schedule.Para)
                .ThenBy(schedule => schedule.Group.Name)
                .ToList();

            ScheduleDayDto dayDto = new ScheduleDayDto
            {
                Day = day,
                DayName = GetDayName(day),
                Date = dayDate,
                IsHoliday = holidaysByDate.ContainsKey(dayDate),
                HolidayName = holidaysByDate.GetValueOrDefault(dayDate)
            };

            foreach (IGrouping<short, Schedule> paraGroup in daySchedules.GroupBy(schedule => schedule.Para).OrderBy(group => group.Key))
            {
                ScheduleParaDto paraDto = new ScheduleParaDto
                {
                    Para = paraGroup.Key,
                    TimeRange = GetParaTimeRange(paraGroup.Key),
                    Lessons = paraGroup.Select(MapLesson).ToList()
                };

                dayDto.Paras.Add(paraDto);
            }

            board.Days.Add(dayDto);
        }

        return board;
    }

    private static ScheduleLessonDto MapLesson(Schedule schedule)
    {
        return new ScheduleLessonDto
        {
            ScheduleId = schedule.Id,
            SubjectName = schedule.Discipline.Subject.Name,
            TeacherFullName = BuildFullName(schedule.Teacher.LastName, schedule.Teacher.FirstName, schedule.Teacher.FatherName),
            GroupName = schedule.Group.Name,
            AudienceNumber = schedule.Audience.Number,
            LectureType = schedule.LectureType?.ToString() ?? string.Empty,
            AudienceType = schedule.Audience.Type.ToString(),
            Day = schedule.Den,
            Para = schedule.Para
        };
    }

    private static WeeklyScheduleBoardDto CreateEmptyBoard(Weeks? week)
    {
        return new WeeklyScheduleBoardDto
        {
            WeekId = week?.Id,
            WeekName = week?.Name ?? string.Empty,
            StartDate = week?.StartDate,
            EndDate = week?.EndDate,
            AcademicYearName = week?.AcademicYear?.Name ?? string.Empty,
            PeriodName = week?.AcademicPeriod?.Name ?? string.Empty,
            PeriodType = week?.AcademicPeriod?.Type.ToString() ?? string.Empty,
            WeekType = week?.Type.ToString() ?? string.Empty
        };
    }

    private static bool IsWithin(DateTime today, Weeks week)
    {
        return week.StartDate.Date <= today.Date && week.EndDate.Date >= today.Date;
    }

    private static string BuildWeekDisplayName(Weeks week)
    {
        string prefix = string.IsNullOrWhiteSpace(week.AcademicPeriod?.Name)
            ? week.Type.ToString()
            : week.AcademicPeriod.Name;

        return $"{prefix} · {week.Name} · {week.StartDate:dd.MM.yyyy} - {week.EndDate:dd.MM.yyyy}";
    }

    private static string GetDayName(int day)
    {
        return day switch
        {
            1 => "Понедельник",
            2 => "Вторник",
            3 => "Среда",
            4 => "Четверг",
            5 => "Пятница",
            6 => "Суббота",
            _ => string.Empty
        };
    }

    private static string GetParaTimeRange(int para)
    {
        return para switch
        {
            1 => "08:00 - 09:20",
            2 => "09:30 - 10:50",
            3 => "11:00 - 12:20",
            4 => "12:40 - 14:00",
            5 => "14:10 - 15:30",
            6 => "15:40 - 17:00",
            7 => "17:10 - 18:30",
            _ => string.Empty
        };
    }

    private static string BuildFullName(string lastName, string firstName, string? fatherName)
    {
        if (string.IsNullOrWhiteSpace(fatherName))
        {
            return $"{lastName} {firstName}";
        }

        return $"{lastName} {firstName} {fatherName}";
    }
}
