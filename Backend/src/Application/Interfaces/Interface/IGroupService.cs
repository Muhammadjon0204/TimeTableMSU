// src/Application/Interfaces/Interface/IGroupService.cs
using Application.Common;
using Application.DTOs.GroupDTOs;

namespace Application.Interfaces.Interface;

public interface IGroupService
{
    Task<Result<GetGroupDetailDto>> GetByIdAsync(int id);
    Task<Result<List<GetGroupDto>>> GetAllAsync();
    Task<Result<GetGroupDto>> CreateAsync(CreateGroupDto dto);
    Task<Result<GetGroupDto>> UpdateAsync(UpdateGroupDto dto);
    Task<Result> DeleteAsync(int id);
}
