// src/Application/Interfaces/Interface/IStudentService.cs
using Application.Common;
using Application.DTOs.StudentDTOs;

namespace Application.Interfaces.Interface;

public interface IStudentService
{
    Task<Result<GetStudentDetailDto>> GetByIdAsync(int id);
    Task<Result<PagedResult<GetStudentDto>>> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm);
    Task<Result<GetStudentDto>> CreateAsync(CreateStudentDto dto);
    Task<Result<GetStudentDto>> UpdateAsync(UpdateStudentDto dto);
    Task<Result> DeleteAsync(int id);
}
