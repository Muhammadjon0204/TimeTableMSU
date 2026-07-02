using Domain.Entities;
using Domain.Enums;

namespace Application.Service;

public static class AcademicWeekGenerator
{
    public static List<Weeks> GenerateStudyWeeks(AcademicYear academicYear, DateOnly startDate, DateOnly generateUntil, int startNumber = 1)
    {
        var weeks = new List<Weeks>();
        DateOnly currentStart = startDate;
        int weekNumber = startNumber;

        while (currentStart <= generateUntil)
        {
            if (currentStart.DayOfWeek == DayOfWeek.Sunday)
            {
                currentStart = currentStart.AddDays(1);
                continue;
            }

            DateOnly currentEnd = GetSaturdayOfWeek(currentStart);
            if (currentEnd > generateUntil)
            {
                currentEnd = generateUntil;
            }

            weeks.Add(new Weeks
            {
                AcademicYear = academicYear,
                AcademicYearId = academicYear.Id == 0 ? null : academicYear.Id,
                Name = $"{weekNumber} учебная неделя",
                StartDate = currentStart.ToDateTime(TimeOnly.MinValue),
                EndDate = currentEnd.ToDateTime(TimeOnly.MinValue),
                Type = WeekType.Study
            });

            weekNumber++;
            currentStart = currentEnd.AddDays(2);
        }

        return weeks;
    }

    public static DateOnly GetDefaultGenerateUntil(AcademicYear academicYear)
    {
        DateOnly defaultGenerateUntil = new(academicYear.StartDate.Year + 1, 7, 20);
        DateOnly academicYearEnd = DateOnly.FromDateTime(academicYear.EndDate);

        return defaultGenerateUntil > academicYearEnd ? academicYearEnd : defaultGenerateUntil;
    }

    private static DateOnly GetSaturdayOfWeek(DateOnly date)
    {
        int daysUntilSaturday = DayOfWeek.Saturday - date.DayOfWeek;

        if (daysUntilSaturday < 0)
        {
            daysUntilSaturday += 7;
        }

        return date.AddDays(daysUntilSaturday);
    }
}
