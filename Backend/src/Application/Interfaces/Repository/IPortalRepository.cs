using Domain.Entities;

namespace Application.Interfaces.Repository;

public interface IPortalRepository
{
    Task<Student?> GetStudentByEmailAsync(string email);
    Task<Teacher?> GetTeacherByEmailAsync(string email);
    Task<List<Schedule>> GetSchedulesByGroupAsync(int groupId);
    Task<List<AcademicPerformance>> GetAcademicPerformancesByStudentAsync(int studentId);
    Task<List<Attendance>> GetAttendancesByStudentAsync(int studentId);
    Task<List<Discipline>> GetDisciplinesByTeacherAsync(int teacherId);
    Task<List<Schedule>> GetSchedulesByTeacherAsync(int teacherId);
    Task<List<Attendance>> GetAttendancesByTeacherAsync(int teacherId);
    Task<List<AcademicPerformance>> GetAcademicPerformancesByTeacherAsync(int teacherId);
    Task<List<Execution>> GetExecutionsByTeacherAsync(int teacherId);
}
