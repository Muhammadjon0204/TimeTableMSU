using Application.Interfaces.Interface;
using Application.Interfaces.Repository;
using Infrastructure.Persistence.Repositories;
using Infrastructure.Security;
using Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureRepositories(this IServiceCollection services)
    {
        services.AddScoped<IFacultyRepository, FacultyRepository>();
        services.AddScoped<ISpecialityRepository, SpecialityRepository>();
        services.AddScoped<IGroupRepository, GroupRepository>();
        services.AddScoped<IStudentRepository, StudentRepository>();
        services.AddScoped<ITeacherRepository, TeacherRepository>();
        services.AddScoped<ISubjectRepository, SubjectRepository>();
        services.AddScoped<IDisciplineRepository, DisciplineRepository>();
        services.AddScoped<IAttendanceRepository, AttendanceRepository>();
        services.AddScoped<IAcademicYearRepository, AcademicYearRepository>();
        services.AddScoped<IWeekRepository, WeekRepository>();
        services.AddScoped<IScheduleRepository, ScheduleRepository>();
        services.AddScoped<IAudienceRepository, AudienceRepository>();
        services.AddScoped<IExecutionRepository, ExecutionRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAcademicPerformanceRepository, AcademicPerformanceRepository>();
        services.AddScoped<IPortalRepository, PortalRepository>();
        services.AddScoped<IAdminDashboardRepository, AdminDashboardRepository>();
        services.AddScoped<IAdminAcademicJournalRepository, AdminAcademicJournalRepository>();
        services.AddScoped<IAdminScheduleBoardRepository, AdminScheduleBoardRepository>();
        services.AddScoped<IAdminScheduleLookupRepository, AdminScheduleLookupRepository>();

        return services;
    }

    public static IServiceCollection AddInfrastructureSecurity(this IServiceCollection services)
    {
        services.AddScoped<PasswordHasher>();
        services.AddScoped<ISmtpService, SmtpService>();
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
