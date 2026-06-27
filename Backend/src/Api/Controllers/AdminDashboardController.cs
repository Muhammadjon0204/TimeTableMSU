using Api.Authorization;
using Application.Interfaces.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("api/admin-dashboard")]
[Authorize(Policy = AuthPolicies.AdminOnly)]
public class AdminDashboardController : ApiControllerBase
{
    private readonly IAdminDashboardService _adminDashboardService;

    public AdminDashboardController(IAdminDashboardService adminDashboardService)
    {
        _adminDashboardService = adminDashboardService;
    }

    [HttpGet("attendance-weekly")]
    public async Task<ActionResult> GetAttendanceWeekly([FromQuery] int? weekId, [FromQuery] int? groupId)
    {
        return ToOkResult(await _adminDashboardService.GetAttendanceWeeklyAsync(weekId, groupId));
    }

    [HttpGet("group-lookups")]
    public async Task<ActionResult> GetGroupLookups()
    {
        return ToOkResult(await _adminDashboardService.GetGroupLookupsAsync());
    }
}
