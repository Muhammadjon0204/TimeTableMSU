// src/Application/Interfaces/Interface/IAttendanceService.cs
using Application.Common;
using Application.DTOs.AttendanceDTOs;

namespace Application.Interfaces.Interface;

public interface IAttendanceService
{
    Task<Result<PagedResult<GetAttendanceDto>>> GetPagedAsync(int pageNumber, int pageSize);
    Task<Result<GetAttendanceDto>> CreateAsync(CreateAttendanceDto dto);
    Task<Result<GetAttendanceDto>> UpdateAsync(UpdateAttendanceDto dto);
    Task<Result> DeleteAsync(int id);
}
