using Api.Authorization;
using Application.DTOs.WeekDTOs;
using Application.Interfaces.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("api/weeks")]
[Authorize(Policy = AuthPolicies.AdminOnly)]
public class WeeksController : ApiControllerBase
{
    private readonly IWeekService _weekService;

    public WeeksController(IWeekService weekService)
    {
        _weekService = weekService;
    }

    [HttpGet]
    public async Task<ActionResult> GetAll()
    {
        return ToOkResult(await _weekService.GetAllAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult> GetById(int id)
    {
        return ToOkResult(await _weekService.GetByIdAsync(id));
    }

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CreateWeekDto dto)
    {
        return ToCreatedResult(await _weekService.CreateAsync(dto));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> Update(int id, [FromBody] UpdateWeekDto dto)
    {
        if (id != dto.Id)
        {
            return IdMismatch();
        }

        return ToOkResult(await _weekService.UpdateAsync(dto));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        return ToNoContentResult(await _weekService.DeleteAsync(id));
    }
}
