// src/Application/Interfaces/Repository/IStudentRepository.cs
using Application.Common;
using Domain.Entities;

namespace Application.Interfaces.Repository;

public interface IStudentRepository
{
    Task<Student?> GetByIdAsync(int id);
    Task<List<Student>> GetAllAsync();
    Task<PagedResult<Student>> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm);
    Task AddAsync(Student student);
    Task UpdateAsync(Student student);
    Task DeleteAsync(Student student);
}
