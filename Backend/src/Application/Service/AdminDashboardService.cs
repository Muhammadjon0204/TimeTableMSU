using Application.Common;
using Application.DTOs.DashboardDTOs;
using Application.Interfaces.Interface;
using Application.Interfaces.Repository;
using Domain.Entities;
using Domain.Enums;

namespace Application.Service;

public class AdminDashboardService : IAdminDashboardService
{
    private readonly IAdminDashboardRepository _adminDashboardRepository;

    public AdminDashboardService(IAdminDashboardRepository adminDashboardRepository)
    {
        _adminDashboardRepository = adminDashboardRepository;
    }

    public async Task<Result<AdminAttendanceWeeklyDto>> GetAttendanceWeeklyAsync(int? weekId, int? groupId)
    {
        if (weekId.HasValue && weekId.Value <= 0)
        {
            return Result<AdminAttendanceWeeklyDto>.Failure("Invalid week id");
        }

        if (groupId.HasValue && groupId.Value <= 0)
        {
            return Result<AdminAttendanceWeeklyDto>.Failure("Invalid group id");
        }

        Weeks? week = await _adminDashboardRepository.GetWeekAsync(weekId);
        List<Group> groups = await _adminDashboardRepository.GetGroupsAsync(groupId);

        if (groupId.HasValue && groups.Count == 0)
        {
            return Result<AdminAttendanceWeeklyDto>.Failure("Group not found");
        }

        AdminAttendanceWeeklyDto dto = new AdminAttendanceWeeklyDto
        {
            WeekId = week?.Id,
            WeekName = week?.Name,
            GroupId = groupId,
            GroupName = groupId.HasValue ? groups.FirstOrDefault()?.Name ?? "Выбранная группа" : "Все группы",
            Days = CreateEmptyDays()
        };

        if (week == null || groups.Count == 0)
        {
            return Result<AdminAttendanceWeeklyDto>.Success(dto);
        }

        List<int> groupIds = groups.Select(g => g.Id).ToList();
        Dictionary<int, int> studentCounts = await _adminDashboardRepository.GetStudentCountsByGroupAsync(groupIds);
        List<Schedule> schedules = await _adminDashboardRepository.GetSchedulesAsync(week.Id, groupIds);
        List<Attendance> attendances = await _adminDashboardRepository.GetAttendancesAsync(week.Id, groupIds);
        HashSet<(int GroupId, int Day, int Para)> scheduledLessonKeys = schedules
            .Select(schedule => (schedule.GroupId, (int)schedule.Den, (int)schedule.Para))
            .ToHashSet();

        Dictionary<(int Day, int GroupId), int> expectedByDayAndGroup = schedules
            .Where(s => s.Den >= 1 && s.Den <= 6)
            .GroupBy(s => ((int)s.Den, s.GroupId))
            .ToDictionary(
                g => g.Key,
                g => g.Sum(schedule => studentCounts.TryGetValue(schedule.GroupId, out int count) ? count : 0));

        Dictionary<int, int> expectedByDay = expectedByDayAndGroup
            .GroupBy(item => item.Key.Day)
            .ToDictionary(g => g.Key, g => g.Sum(item => item.Value));

        Dictionary<int, AttendanceCount> countsByDay = CountAttendanceByDay(attendances, scheduledLessonKeys);

        foreach (AdminAttendanceDayDto day in dto.Days)
        {
            int expected = expectedByDay.GetValueOrDefault(day.Day);
            AttendanceCount counts = countsByDay.GetValueOrDefault(day.Day);
            int present = Math.Max(expected - counts.Absent - counts.Late, 0);

            day.Expected = expected;
            day.Absent = counts.Absent;
            day.Late = counts.Late;
            day.Present = present;
            day.AttendancePercent = expected == 0 ? 0 : Math.Round((double)present / expected * 100, 1);
        }

        return Result<AdminAttendanceWeeklyDto>.Success(dto);
    }

    public async Task<Result<List<GroupLookupDto>>> GetGroupLookupsAsync()
    {
        List<Group> groups = await _adminDashboardRepository.GetGroupsAsync(null);
        List<GroupLookupDto> dtos = groups
            .Select(group => new GroupLookupDto
            {
                Id = group.Id,
                Name = group.Name
            })
            .ToList();

        return Result<List<GroupLookupDto>>.Success(dtos);
    }

    private static List<AdminAttendanceDayDto> CreateEmptyDays()
    {
        return new List<AdminAttendanceDayDto>
        {
            new() { Day = 1, DayName = "Пн" },
            new() { Day = 2, DayName = "Вт" },
            new() { Day = 3, DayName = "Ср" },
            new() { Day = 4, DayName = "Чт" },
            new() { Day = 5, DayName = "Пт" },
            new() { Day = 6, DayName = "Сб" }
        };
    }

    private static Dictionary<int, AttendanceCount> CountAttendanceByDay(
        List<Attendance> attendances,
        HashSet<(int GroupId, int Day, int Para)> scheduledLessonKeys)
    {
        Dictionary<int, AttendanceCount> result = new();

        IEnumerable<IGrouping<(int StudentId, int WeekId, int Day, int Para), Attendance>> groups = attendances
            .Where(a =>
                a.Day >= 1 &&
                a.Day <= 6 &&
                a.Student.GroupId.HasValue &&
                scheduledLessonKeys.Contains((a.Student.GroupId.Value, (int)a.Day, (int)a.Para)))
            .GroupBy(a => (a.StudentId, a.WeekId, (int)a.Day, (int)a.Para));

        foreach (IGrouping<(int StudentId, int WeekId, int Day, int Para), Attendance> group in groups)
        {
            AttendanceCount count = result.GetValueOrDefault(group.Key.Day);

            if (group.Any(IsAbsent))
            {
                count.Absent += 1;
            }
            else if (group.Any(IsLate))
            {
                count.Late += 1;
            }

            result[group.Key.Day] = count;
        }

        return result;
    }

    private static bool IsAbsent(Attendance attendance)
    {
        // TODO: ValidReason is included in absent for this chart; later it can become a separate line.
        return attendance.Mark == AttendanceMark.Absent ||
               attendance.Mark == AttendanceMark.ValidReason ||
               attendance.Mark == AttendanceMark.None;
    }

    private static bool IsLate(Attendance attendance)
    {
        // Attendance currently has no LateMinutes field; Late is based on the enum value.
        return attendance.Mark == AttendanceMark.Late;
    }

    private struct AttendanceCount
    {
        public int Late { get; set; }
        public int Absent { get; set; }
    }
}
