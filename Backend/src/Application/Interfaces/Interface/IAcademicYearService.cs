using Application.Common;
using Application.DTOs.AcademicYearDTOs;

namespace Application.Interfaces.Interface;

public interface IAcademicYearService
{
    Task<Result<List<AcademicYearDto>>> GetAllAsync();
    Task<Result<AcademicYearDto>> GetCurrentAsync();
    Task<Result<AcademicYearDto>> GetByIdAsync(int id);
    Task<Result<AcademicYearDto>> CreateAsync(CreateAcademicYearDto dto);
    Task<Result<AcademicYearDto>> UpdateAsync(UpdateAcademicYearDto dto);
    Task<Result> DeleteAsync(int id);
    Task<Result<GenerateWeeksResponse>> GenerateWeeksAsync(int academicYearId, GenerateWeeksRequest request);
    Task<Result<AcademicYearDto>> RecoverFromWeeksAsync();
}
