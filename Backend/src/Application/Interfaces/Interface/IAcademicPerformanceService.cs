// src/Application/Interfaces/Interface/IAcademicPerformanceService.cs
using Application.Common;
using Application.DTOs.AcademicPerformanceDTOs;

namespace Application.Interfaces.Interface;

public interface IAcademicPerformanceService
{
    Task<Result<GetAcademicPerformanceDto>> GetByIdAsync(int id);
    Task<Result<PagedResult<GetAcademicPerformanceDto>>> GetPagedAsync(int pageNumber, int pageSize);
    Task<Result<GetAcademicPerformanceDto>> CreateAsync(CreateAcademicPerformanceDto dto);
    Task<Result<GetAcademicPerformanceDto>> UpdateAsync(UpdateAcademicPerformanceDto dto);
    Task<Result> DeleteAsync(int id);
}
