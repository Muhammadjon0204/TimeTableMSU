using Api.Authorization;
using Application.Interfaces.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("api/admin-academic-journal")]
[Authorize(Policy = AuthPolicies.AdminOnly)]
public class AdminAcademicJournalController : ApiControllerBase
{
    private readonly IAdminAcademicJournalService _service;

    public AdminAcademicJournalController(IAdminAcademicJournalService service)
    {
        _service = service;
    }

    [HttpGet("faculties")]
    public async Task<ActionResult> GetFaculties()
    {
        return ToOkResult(await _service.GetFacultiesAsync());
    }

    [HttpGet("faculties/{facultyId:int}/groups")]
    public async Task<ActionResult> GetGroupsByFaculty(int facultyId)
    {
        return ToOkResult(await _service.GetGroupsByFacultyAsync(facultyId));
    }

    [HttpGet("groups/{groupId:int}/students")]
    public async Task<ActionResult> GetStudentsByGroup(int groupId)
    {
        return ToOkResult(await _service.GetStudentsByGroupAsync(groupId));
    }

    [HttpGet("students/{studentId:int}/gradebook")]
    public async Task<ActionResult> GetStudentGradebook(int studentId)
    {
        return ToOkResult(await _service.GetStudentGradebookAsync(studentId));
    }

    [HttpGet("students/{studentId:int}/journal")]
    public async Task<ActionResult> GetStudentJournal(int studentId)
    {
        return ToOkResult(await _service.GetStudentJournalAsync(studentId));
    }
}
