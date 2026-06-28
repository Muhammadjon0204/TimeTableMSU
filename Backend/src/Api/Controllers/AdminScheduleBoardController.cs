using Api.Authorization;
using Application.Interfaces.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("api/admin-schedule-board")]
[Authorize(Policy = AuthPolicies.AdminOnly)]
public class AdminScheduleBoardController : ApiControllerBase
{
    private readonly IAdminScheduleBoardService _service;

    public AdminScheduleBoardController(IAdminScheduleBoardService service)
    {
        _service = service;
    }

    [HttpGet("weekly")]
    public async Task<ActionResult> GetWeekly([FromQuery] int? weekId, [FromQuery] int? groupId, [FromQuery] int? teacherId, [FromQuery] int? audienceId)
    {
        return ToOkResult(await _service.GetWeeklyAsync(weekId, groupId, teacherId, audienceId));
    }

    [HttpGet("week-lookups")]
    public async Task<ActionResult> GetWeekLookups()
    {
        return ToOkResult(await _service.GetWeekLookupsAsync());
    }

    [HttpGet("group-lookups")]
    public async Task<ActionResult> GetGroupLookups()
    {
        return ToOkResult(await _service.GetGroupLookupsAsync());
    }

    [HttpGet("teacher-lookups")]
    public async Task<ActionResult> GetTeacherLookups()
    {
        return ToOkResult(await _service.GetTeacherLookupsAsync());
    }

    [HttpGet("audience-lookups")]
    public async Task<ActionResult> GetAudienceLookups()
    {
        return ToOkResult(await _service.GetAudienceLookupsAsync());
    }
}
