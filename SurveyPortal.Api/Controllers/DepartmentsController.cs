using Microsoft.AspNetCore.Mvc;
using SurveyPortal.Api.Dtos;
using SurveyPortal.Api.Repositories.Interface;

namespace SurveyPortal.Api.Controllers;

[ApiController]
[Route("departments")]
public class DepartmentsController(IDepartmentRepository departments) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<DepartmentDto>>> GetAll(int surveyId, int raterId)
    {
        var all = await departments.GetAllAsync(surveyId, raterId);
        return Ok(all);
    }

    // Unit completion is scoped to the rater's own submissions for this survey.
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, int surveyId, int raterId)
    {
        var department = await departments.GetDetailAsync(id, surveyId, raterId);
        return department is null ? NotFound() : Ok(department);
    }
}
