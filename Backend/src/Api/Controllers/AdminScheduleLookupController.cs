using Api.Authorization;
using Application.Interfaces.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("api/admin-schedule-lookups")]
[Authorize(Policy = AuthPolicies.AdminOnly)]
public class AdminScheduleLookupController : ApiControllerBase
{
    private readonly IAdminScheduleLookupService _service;

    public AdminScheduleLookupController(IAdminScheduleLookupService service)
    {
        _service = service;
    }

    [HttpGet("weeks")]
    public async Task<ActionResult> GetWeeks()
    {
        return ToOkResult(await _service.GetWeeksAsync());
    }

    [HttpGet("disciplines")]
    public async Task<ActionResult> GetDisciplines()
    {
        return ToOkResult(await _service.GetDisciplinesAsync());
    }

    [HttpGet("discipline-options")]
    public async Task<ActionResult> GetDisciplineOptions([FromQuery] int subjectId)
    {
        return ToOkResult(await _service.GetDisciplineOptionsAsync(subjectId));
    }

    [HttpGet("audiences")]
    public async Task<ActionResult> GetAudiences([FromQuery] string? lectureType)
    {
        return ToOkResult(await _service.GetAudiencesAsync(lectureType));
    }
}
