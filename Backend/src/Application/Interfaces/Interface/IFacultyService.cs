using Application.Common;
using Application.DTOs.FacultyDTOs;

namespace Application.Interfaces.Interface;

public interface IFacultyService
{
    Task<Result<GetFacultyDto>> GetByIdAsync(int id);
    Task<Result<List<GetFacultyDto>>> GetAllAsync();
    Task<Result<GetFacultyDto>> CreateAsync(CreateFacultyDto dto);
    Task<Result<GetFacultyDto>> UpdateAsync(UpdateFacultyDto dto);
    Task<Result> DeleteAsync(int id);
}
