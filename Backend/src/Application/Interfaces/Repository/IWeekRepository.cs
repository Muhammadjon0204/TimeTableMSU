// src/Application/Interfaces/Repository/IWeekRepository.cs
using Domain.Entities;

namespace Application.Interfaces.Repository;

public interface IWeekRepository
{
    Task<Weeks?> GetByIdAsync(int id);
    Task<List<Weeks>> GetAllAsync();
    Task<bool> HasDateIntersectionAsync(DateTime startDate, DateTime endDate, int? excludedId = null);
    Task AddAsync(Weeks week);
    Task UpdateAsync(Weeks week);
    Task DeleteAsync(Weeks week);
}
