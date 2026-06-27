using Api.Authorization;
using Application.DTOs.AcademicPerformanceDTOs;
using Application.Interfaces.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("api/academic-performances")]
[Authorize(Policy = AuthPolicies.AdminOrTeacher)]
public class AcademicPerformancesController : ApiControllerBase
{
    private readonly IAcademicPerformanceService _academicPerformanceService;

    public AcademicPerformancesController(IAcademicPerformanceService academicPerformanceService)
    {
        _academicPerformanceService = academicPerformanceService;
    }

    [HttpGet]
    public async Task<ActionResult> GetPaged([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        return ToOkResult(await _academicPerformanceService.GetPagedAsync(pageNumber, pageSize));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult> GetById(int id)
    {
        return ToOkResult(await _academicPerformanceService.GetByIdAsync(id));
    }

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CreateAcademicPerformanceDto dto)
    {
        return ToCreatedResult(await _academicPerformanceService.CreateAsync(dto));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> Update(int id, [FromBody] UpdateAcademicPerformanceDto dto)
    {
        if (id != dto.Id)
        {
            return IdMismatch();
        }

        return ToOkResult(await _academicPerformanceService.UpdateAsync(dto));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        return ToNoContentResult(await _academicPerformanceService.DeleteAsync(id));
    }
}
