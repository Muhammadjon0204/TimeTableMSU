using Application.Interfaces.Interface;
using Application.Service;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IFacultyService, FacultyService>();
        services.AddScoped<ISpecialityService, SpecialityService>();
        services.AddScoped<IGroupService, GroupService>();
        services.AddScoped<IStudentService, StudentService>();
        services.AddScoped<ITeacherService, TeacherService>();
        services.AddScoped<ISubjectService, SubjectService>();
        services.AddScoped<IDisciplineService, DisciplineService>();
        services.AddScoped<IAudienceService, AudienceService>();
        services.AddScoped<IAcademicYearService, AcademicYearService>();
        services.AddScoped<IWeekService, WeekService>();
        services.AddScoped<IScheduleService, ScheduleService>();
        services.AddScoped<IAttendanceService, AttendanceService>();
        services.AddScoped<IAcademicPerformanceService, AcademicPerformanceService>();
        services.AddScoped<IExecutionService, ExecutionService>();
        services.AddScoped<IPortalService, PortalService>();
        services.AddScoped<IAdminDashboardService, AdminDashboardService>();
        services.AddScoped<IAdminAcademicJournalService, AdminAcademicJournalService>();
        services.AddScoped<IAdminScheduleBoardService, AdminScheduleBoardService>();
        services.AddScoped<IAdminScheduleLookupService, AdminScheduleLookupService>();

        return services;
    }
}
