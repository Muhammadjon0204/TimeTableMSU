using Api.Authorization;
using Application.DTOs.AttendanceDTOs;
using Application.Interfaces.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("api/attendances")]
[Authorize(Policy = AuthPolicies.AdminOrTeacher)]
public class AttendancesController : ApiControllerBase
{
    private readonly IAttendanceService _attendanceService;

    public AttendancesController(IAttendanceService attendanceService)
    {
        _attendanceService = attendanceService;
    }

    [HttpGet]
    public async Task<ActionResult> GetPaged([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        return ToOkResult(await _attendanceService.GetPagedAsync(pageNumber, pageSize));
    }

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CreateAttendanceDto dto)
    {
        return ToCreatedResult(await _attendanceService.CreateAsync(dto));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> Update(int id, [FromBody] UpdateAttendanceDto dto)
    {
        if (id != dto.Id)
        {
            return IdMismatch();
        }

        return ToOkResult(await _attendanceService.UpdateAsync(dto));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        return ToNoContentResult(await _attendanceService.DeleteAsync(id));
    }
}
