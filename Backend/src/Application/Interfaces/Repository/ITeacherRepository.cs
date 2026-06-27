// src/Application/Interfaces/Repository/ITeacherRepository.cs
using Domain.Entities;

namespace Application.Interfaces.Repository;

public interface ITeacherRepository
{
    Task<Teacher?> GetByIdAsync(int id);
    Task<List<Teacher>> GetAllAsync();
    Task<bool> ExistsByEmailAsync(string email, int? excludedId = null);
    Task<bool> ExistsByFullNameAsync(string firstName, string lastName, string? fatherName, int? excludedId = null);
    Task AddAsync(Teacher teacher);
    Task UpdateAsync(Teacher teacher);
    Task DeleteAsync(Teacher teacher);
}
