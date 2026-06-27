using Api.Authorization;
using Application.DTOs.ScheduleDTOs;
using Application.Interfaces.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("api/schedules")]
[Authorize(Policy = AuthPolicies.AdminOnly)]
public class SchedulesController : ApiControllerBase
{
    private readonly IScheduleService _scheduleService;

    public SchedulesController(IScheduleService scheduleService)
    {
        _scheduleService = scheduleService;
    }

    [HttpGet]
    public async Task<ActionResult> GetPaged([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        return ToOkResult(await _scheduleService.GetPagedAsync(pageNumber, pageSize));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult> GetById(int id)
    {
        return ToOkResult(await _scheduleService.GetByIdAsync(id));
    }

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CreateScheduleDto dto)
    {
        return ToCreatedIdResult(await _scheduleService.CreateAsync(dto));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> Update(int id, [FromBody] UpdateScheduleDto dto)
    {
        if (id != dto.Id)
        {
            return IdMismatch();
        }

        return ToOkResult(await _scheduleService.UpdateAsync(dto));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        return ToNoContentResult(await _scheduleService.DeleteAsync(id));
    }
}
