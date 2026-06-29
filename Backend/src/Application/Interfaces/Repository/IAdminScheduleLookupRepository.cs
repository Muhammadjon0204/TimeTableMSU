using Domain.Entities;

namespace Application.Interfaces.Repository;

public interface IAdminScheduleLookupRepository
{
    Task<List<Weeks>> GetWeeksAsync();
    Task<List<Subject>> GetSubjectsWithDisciplinesAsync();
    Task<List<Discipline>> GetDisciplineOptionsAsync(int subjectId);
    Task<List<Audience>> GetAudiencesAsync();
}
