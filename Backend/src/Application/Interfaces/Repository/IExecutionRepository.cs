// src/Application/Interfaces/Repository/IExecutionRepository.cs
using Domain.Entities;

namespace Application.Interfaces.Repository;

public interface IExecutionRepository
{
    Task<Execution?> GetByIdAsync(int id);
    Task<List<Execution>> GetAllAsync();
    Task AddAsync(Execution execution);
    Task UpdateAsync(Execution execution);
    Task DeleteAsync(Execution execution);
}
