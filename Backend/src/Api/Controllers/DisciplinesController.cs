using Api.Authorization;
using Application.DTOs.DisciplineDTOs;
using Application.Interfaces.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("api/disciplines")]
[Authorize(Policy = AuthPolicies.AdminOnly)]
public class DisciplinesController : ApiControllerBase
{
    private readonly IDisciplineService _disciplineService;

    public DisciplinesController(IDisciplineService disciplineService)
    {
        _disciplineService = disciplineService;
    }

    [HttpGet]
    public async Task<ActionResult> GetAll()
    {
        return ToOkResult(await _disciplineService.GetAllAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult> GetById(int id)
    {
        return ToOkResult(await _disciplineService.GetByIdAsync(id));
    }

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CreateDisciplineDto dto)
    {
        return ToCreatedResult(await _disciplineService.CreateAsync(dto));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> Update(int id, [FromBody] UpdateDisciplineDto dto)
    {
        if (id != dto.Id)
        {
            return IdMismatch();
        }

        return ToOkResult(await _disciplineService.UpdateAsync(dto));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        return ToNoContentResult(await _disciplineService.DeleteAsync(id));
    }
}
