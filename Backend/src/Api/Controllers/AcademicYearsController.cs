using Api.Authorization;
using Application.DTOs.AcademicYearDTOs;
using Application.Interfaces.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("api/academic-years")]
[Authorize(Policy = AuthPolicies.AdminOnly)]
public class AcademicYearsController : ApiControllerBase
{
    private readonly IAcademicYearService _academicYearService;

    public AcademicYearsController(IAcademicYearService academicYearService)
    {
        _academicYearService = academicYearService;
    }

    [HttpGet]
    public async Task<ActionResult> GetAll()
    {
        return ToOkResult(await _academicYearService.GetAllAsync());
    }

    [HttpGet("current")]
    public async Task<ActionResult> GetCurrent()
    {
        return ToOkResult(await _academicYearService.GetCurrentAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult> GetById(int id)
    {
        return ToOkResult(await _academicYearService.GetByIdAsync(id));
    }

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CreateAcademicYearDto dto)
    {
        return ToCreatedResult(await _academicYearService.CreateAsync(dto));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> Update(int id, [FromBody] UpdateAcademicYearDto dto)
    {
        if (id != dto.Id)
        {
            return IdMismatch();
        }

        return ToOkResult(await _academicYearService.UpdateAsync(dto));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        return ToNoContentResult(await _academicYearService.DeleteAsync(id));
    }

    [HttpPost("{id:int}/generate-weeks")]
    public async Task<ActionResult> GenerateWeeks(int id, [FromBody] GenerateWeeksRequest request)
    {
        return ToOkResult(await _academicYearService.GenerateWeeksAsync(id, request));
    }

    [HttpPost("recover-from-weeks")]
    public async Task<ActionResult> RecoverFromWeeks()
    {
        return ToOkResult(await _academicYearService.RecoverFromWeeksAsync());
    }
}
