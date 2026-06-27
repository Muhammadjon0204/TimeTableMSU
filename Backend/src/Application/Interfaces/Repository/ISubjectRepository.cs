// src/Application/Interfaces/Repository/ISubjectRepository.cs
using Domain.Entities;

namespace Application.Interfaces.Repository;

public interface ISubjectRepository
{
    Task<Subject?> GetByIdAsync(int id);
    Task<List<Subject>> GetAllAsync();
    Task<bool> ExistsByNameAndSemesterAsync(string name, short semester, int? excludedId = null);
    Task AddAsync(Subject subject);
    Task UpdateAsync(Subject subject);
    Task DeleteAsync(Subject subject);
}
