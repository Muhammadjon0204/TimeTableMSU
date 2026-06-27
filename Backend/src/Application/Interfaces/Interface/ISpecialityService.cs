using Application.Common;
using Application.DTOs.SpecialityDTOs;

namespace Application.Interfaces.Interface;

public interface ISpecialityService
{
    Task<Result<GetSpecialityDto>> GetByIdAsync(int id);
    Task<Result<List<GetSpecialityDto>>> GetAllAsync();
    Task<Result<GetSpecialityDto>> CreateAsync(CreateSpecialityDto dto);
    Task<Result<GetSpecialityDto>> UpdateAsync(UpdateSpecialityDto dto);
    Task<Result> DeleteAsync(int id);
}
