using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Api.Authorization;
using Application.Interfaces.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("api/teacher-portal")]
[Authorize(Policy = AuthPolicies.TeacherOnly)]
public class TeacherPortalController : ApiControllerBase
{
    private readonly IPortalService _portalService;

    public TeacherPortalController(IPortalService portalService)
    {
        _portalService = portalService;
    }

    [HttpGet("me")]
    public async Task<ActionResult> GetMe()
    {
        string? email = GetCurrentUserEmail();

        if (string.IsNullOrWhiteSpace(email))
        {
            return Unauthorized(new { error = "Current user email claim is missing" });
        }

        var result = await _portalService.GetTeacherProfileAsync(email);

        return result.IsSuccess ? Ok(result.Value) : NotFound(new { error = result.Error });
    }

    [HttpGet("schedule")]
    public async Task<ActionResult> GetSchedule()
    {
        string? email = GetCurrentUserEmail();

        if (string.IsNullOrWhiteSpace(email))
        {
            return Unauthorized(new { error = "Current user email claim is missing" });
        }

        var result = await _portalService.GetTeacherScheduleAsync(email);

        return result.IsSuccess ? Ok(result.Value) : NotFound(new { error = result.Error });
    }

    [HttpGet("disciplines")]
    public async Task<ActionResult> GetDisciplines()
    {
        string? email = GetCurrentUserEmail();

        if (string.IsNullOrWhiteSpace(email))
        {
            return Unauthorized(new { error = "Current user email claim is missing" });
        }

        var result = await _portalService.GetTeacherDisciplinesAsync(email);

        return result.IsSuccess ? Ok(result.Value) : NotFound(new { error = result.Error });
    }

    [HttpGet("attendances")]
    public async Task<ActionResult> GetAttendances()
    {
        string? email = GetCurrentUserEmail();

        if (string.IsNullOrWhiteSpace(email))
        {
            return Unauthorized(new { error = "Current user email claim is missing" });
        }

        var result = await _portalService.GetTeacherAttendancesAsync(email);

        return result.IsSuccess ? Ok(result.Value) : NotFound(new { error = result.Error });
    }

    [HttpGet("academic-performances")]
    public async Task<ActionResult> GetAcademicPerformances()
    {
        string? email = GetCurrentUserEmail();

        if (string.IsNullOrWhiteSpace(email))
        {
            return Unauthorized(new { error = "Current user email claim is missing" });
        }

        var result = await _portalService.GetTeacherAcademicPerformancesAsync(email);

        return result.IsSuccess ? Ok(result.Value) : NotFound(new { error = result.Error });
    }

    [HttpGet("executions")]
    public async Task<ActionResult> GetExecutions()
    {
        string? email = GetCurrentUserEmail();

        if (string.IsNullOrWhiteSpace(email))
        {
            return Unauthorized(new { error = "Current user email claim is missing" });
        }

        var result = await _portalService.GetTeacherExecutionsAsync(email);

        return result.IsSuccess ? Ok(result.Value) : NotFound(new { error = result.Error });
    }

    private string? GetCurrentUserEmail()
    {
        return User.FindFirstValue(ClaimTypes.Email)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Email);
    }
}
