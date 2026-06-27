// src/Application/Interfaces/Interface/IAudienceService.cs
using Application.Common;
using Application.DTOs.AudienceDTOs;

namespace Application.Interfaces.Interface;

public interface IAudienceService
{
    Task<Result<GetAudienceDto>> GetByIdAsync(int id);
    Task<Result<List<GetAudienceDto>>> GetAllAsync();
    Task<Result<GetAudienceDto>> CreateAsync(CreateAudienceDto dto);
    Task<Result<GetAudienceDto>> UpdateAsync(UpdateAudienceDto dto);
    Task<Result> DeleteAsync(int id);
}
