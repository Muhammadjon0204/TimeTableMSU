// src/Application/Interfaces/Interface/IScheduleService.cs
using Application.Common;
using Application.DTOs.ScheduleDTOs;

namespace Application.Interfaces.Interface;

public interface IScheduleService
{
    Task<Result<GetScheduleDto>> GetByIdAsync(int id);
    Task<Result<PagedResult<GetScheduleDto>>> GetPagedAsync(int pageNumber, int pageSize);
    Task<Result<int>> CreateAsync(CreateScheduleDto dto);
    Task<Result<GetScheduleDto>> UpdateAsync(UpdateScheduleDto dto);
    Task<Result> DeleteAsync(int id);
}
