// src/Application/Interfaces/Interface/IWeekService.cs
using Application.Common;
using Application.DTOs.WeekDTOs;

namespace Application.Interfaces.Interface;

public interface IWeekService
{
    Task<Result<GetWeekDto>> GetByIdAsync(int id);
    Task<Result<List<GetWeekDto>>> GetAllAsync();
    Task<Result<GetWeekDto>> CreateAsync(CreateWeekDto dto);
    Task<Result<GetWeekDto>> UpdateAsync(UpdateWeekDto dto);
    Task<Result> DeleteAsync(int id);
}
