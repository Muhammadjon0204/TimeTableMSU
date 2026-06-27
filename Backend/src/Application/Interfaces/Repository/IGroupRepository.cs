// src/Application/Interfaces/Repository/IGroupRepository.cs
using Domain.Entities;

namespace Application.Interfaces.Repository;

public interface IGroupRepository
{
    Task<Group?> GetByIdAsync(int id);
    Task<List<Group>> GetAllAsync();
    Task<bool> ExistsAsync(string name, int specialityId, int? excludedId = null);
    Task AddAsync(Group group);
    Task UpdateAsync(Group group);
    Task DeleteAsync(Group group);
}
