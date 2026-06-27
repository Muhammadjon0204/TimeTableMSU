using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Api.Authorization;
using Application.Interfaces.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("api/student-portal")]
[Authorize(Policy = AuthPolicies.StudentOnly)]
public class StudentPortalController : ApiControllerBase
{
    private readonly IPortalService _portalService;

    public StudentPortalController(IPortalService portalService)
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

        var result = await _portalService.GetStudentProfileAsync(email);

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

        var result = await _portalService.GetStudentScheduleAsync(email);

        return result.IsSuccess ? Ok(result.Value) : NotFound(new { error = result.Error });
    }

    [HttpGet("marks")]
    public async Task<ActionResult> GetMarks()
    {
        string? email = GetCurrentUserEmail();

        if (string.IsNullOrWhiteSpace(email))
        {
            return Unauthorized(new { error = "Current user email claim is missing" });
        }

        var result = await _portalService.GetStudentMarksAsync(email);

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

        var result = await _portalService.GetStudentAttendancesAsync(email);

        return result.IsSuccess ? Ok(result.Value) : NotFound(new { error = result.Error });
    }

    private string? GetCurrentUserEmail()
    {
        return User.FindFirstValue(ClaimTypes.Email)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Email);
    }
}
