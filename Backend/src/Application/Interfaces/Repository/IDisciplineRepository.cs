// src/Application/Interfaces/Repository/IDisciplineRepository.cs
using Domain.Entities;

namespace Application.Interfaces.Repository;

public interface IDisciplineRepository
{
    Task<Discipline?> GetByIdAsync(int id);
    Task<List<Discipline>> GetAllAsync();
    Task<bool> ExistsAsync(int subjectId, int teacherId, int groupId, int? excludedId = null);
    Task AddAsync(Discipline discipline);
    Task UpdateAsync(Discipline discipline);
    Task DeleteAsync(Discipline discipline);
}
