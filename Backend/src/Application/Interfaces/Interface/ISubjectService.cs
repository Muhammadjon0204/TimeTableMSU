// src/Application/Interfaces/Interface/ISubjectService.cs
using Application.Common;
using Application.DTOs.SubjectDTOs;

namespace Application.Interfaces.Interface;

public interface ISubjectService
{
    Task<Result<GetSubjectDto>> GetByIdAsync(int id);
    Task<Result<List<GetSubjectDto>>> GetAllAsync();
    Task<Result<GetSubjectDto>> CreateAsync(CreateSubjectDto dto);
    Task<Result<GetSubjectDto>> UpdateAsync(UpdateSubjectDto dto);
    Task<Result> DeleteAsync(int id);
}
