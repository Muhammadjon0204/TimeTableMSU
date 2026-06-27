// src/Application/Interfaces/Interface/IDisciplineService.cs
using Application.Common;
using Application.DTOs.DisciplineDTOs;

namespace Application.Interfaces.Interface;

public interface IDisciplineService
{
    Task<Result<GetDisciplineDto>> GetByIdAsync(int id);
    Task<Result<List<GetDisciplineDto>>> GetAllAsync();
    Task<Result<GetDisciplineDto>> CreateAsync(CreateDisciplineDto dto);
    Task<Result<GetDisciplineDto>> UpdateAsync(UpdateDisciplineDto dto);
    Task<Result> DeleteAsync(int id);
}
