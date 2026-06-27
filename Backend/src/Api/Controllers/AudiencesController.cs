using Api.Authorization;
using Application.DTOs.AudienceDTOs;
using Application.Interfaces.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("api/audiences")]
[Authorize(Policy = AuthPolicies.AdminOnly)]
public class AudiencesController : ApiControllerBase
{
    private readonly IAudienceService _audienceService;

    public AudiencesController(IAudienceService audienceService)
    {
        _audienceService = audienceService;
    }

    [HttpGet]
    public async Task<ActionResult> GetAll()
    {
        return ToOkResult(await _audienceService.GetAllAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult> GetById(int id)
    {
        return ToOkResult(await _audienceService.GetByIdAsync(id));
    }

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CreateAudienceDto dto)
    {
        return ToCreatedResult(await _audienceService.CreateAsync(dto));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> Update(int id, [FromBody] UpdateAudienceDto dto)
    {
        if (id != dto.Id)
        {
            return IdMismatch();
        }

        return ToOkResult(await _audienceService.UpdateAsync(dto));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        return ToNoContentResult(await _audienceService.DeleteAsync(id));
    }
}
