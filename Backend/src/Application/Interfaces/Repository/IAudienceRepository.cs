// src/Application/Interfaces/Repository/IAudienceRepository.cs
using Domain.Entities;

namespace Application.Interfaces.Repository;

public interface IAudienceRepository
{
    Task<Audience?> GetByIdAsync(int id);
    Task<List<Audience>> GetAllAsync();
    Task<bool> ExistsByNumberAsync(string number, int? excludedId = null);
    Task AddAsync(Audience audience);
    Task UpdateAsync(Audience audience);
    Task DeleteAsync(Audience audience);
}
