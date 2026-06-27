// src/Application/Interfaces/Interface/IExecutionService.cs
using Application.Common;
using Application.DTOs.ExecutionDTOs;

namespace Application.Interfaces.Interface;

public interface IExecutionService
{
    Task<Result<GetExecutionDto>> GetByIdAsync(int id);
    Task<Result<List<GetExecutionDto>>> GetAllAsync();
    Task<Result<int>> CreateAsync(CreateExecutionDto dto);
    Task<Result<GetExecutionDto>> UpdateAsync(UpdateExecutionDto dto);
    Task<Result> DeleteAsync(int id);
}
