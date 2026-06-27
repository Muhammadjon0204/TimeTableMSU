using Api.Authorization;
using Application.DTOs.SubjectDTOs;
using Application.Interfaces.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("api/subjects")]
[Authorize(Policy = AuthPolicies.AdminOnly)]
public class SubjectsController : ApiControllerBase
{
    private readonly ISubjectService _subjectService;

    public SubjectsController(ISubjectService subjectService)
    {
        _subjectService = subjectService;
    }

    [HttpGet]
    public async Task<ActionResult> GetAll()
    {
        return ToOkResult(await _subjectService.GetAllAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult> GetById(int id)
    {
        return ToOkResult(await _subjectService.GetByIdAsync(id));
    }

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CreateSubjectDto dto)
    {
        return ToCreatedResult(await _subjectService.CreateAsync(dto));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> Update(int id, [FromBody] UpdateSubjectDto dto)
    {
        if (id != dto.Id)
        {
            return IdMismatch();
        }

        return ToOkResult(await _subjectService.UpdateAsync(dto));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        return ToNoContentResult(await _subjectService.DeleteAsync(id));
    }
}
