// src/Application/Interfaces/Interface/ITeacherService.cs
using Application.Common;
using Application.DTOs.TeacherDTOs;

namespace Application.Interfaces.Interface;

public interface ITeacherService
{
    Task<Result<GetTeacherDto>> GetByIdAsync(int id);
    Task<Result<List<GetTeacherDto>>> GetAllAsync();
    Task<Result<GetTeacherDto>> CreateAsync(CreateTeacherDto dto);
    Task<Result<GetTeacherDto>> UpdateAsync(UpdateTeacherDto dto);
    Task<Result> DeleteAsync(int id);
}
