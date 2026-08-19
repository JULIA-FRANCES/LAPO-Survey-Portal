using Microsoft.AspNetCore.Mvc;
using SurveyPortal.Api.Dtos;
using SurveyPortal.Api.Models;
using SurveyPortal.Api.Repositories.Interface;

namespace SurveyPortal.Api.Controllers;

[ApiController]
[Route("surveys")]
public class SurveysController(
    ISurveyRepository surveys,
    IQuestionRepository questions,
    IDeptSurveyAssignmentRepository assignments,
    IUserRepository users,
    ISubmissionRepository submissions) : ControllerBase
{
    private static string ComputeStatus(Survey survey)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        if (today < survey.StartDate) return "upcoming";
        if (today > survey.EndDate) return "closed";
        return "open";
    }

    private static SurveyDto ToDto(Survey survey) =>
        new(survey.Id, survey.Name, survey.StartDate, survey.EndDate, ComputeStatus(survey));

    [HttpPost]
    public async Task<IActionResult> Create(CreateSurveyDto newSurvey)
    {
        var survey = new Survey
        {
            Name = newSurvey.Name,
            StartDate = newSurvey.StartDate,
            EndDate = newSurvey.EndDate
        };

        await surveys.AddAsync(survey);

        return CreatedAtAction(nameof(GetById), new { id = survey.Id }, ToDto(survey));
    }

    [HttpGet]
    public async Task<ActionResult<List<SurveyDto>>> GetAll()
    {
        var all = await surveys.GetAllAsync();
        return Ok(all.Select(ToDto).ToList());
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var survey = await surveys.GetByIdAsync(id);
        return survey is null ? NotFound() : Ok(ToDto(survey));
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActive()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var survey = await surveys.GetActiveAsync(today);
        return survey is null ? NotFound() : Ok(ToDto(survey));
    }

    // Departments in scope for a rater department: distinct rated departments
    // assigned to it for this survey.
    [HttpGet("{surveyId:int}/departments")]
    public async Task<ActionResult<List<DepartmentDto>>> GetDepartmentsInScope(int surveyId, int raterDepartmentId)
    {
        var rated = await assignments.GetRatedDepartmentsAsync(surveyId, raterDepartmentId);
        return Ok(rated.Select(d => new DepartmentDto(d.Id, d.Name, d.Units.Count)).ToList());
    }

    [HttpGet("{surveyId:int}/questions")]
    public async Task<ActionResult<List<QuestionDto>>> GetQuestions(int surveyId)
    {
        var list = await questions.GetBySurveyAsync(surveyId);
        return Ok(list.Select(q => new QuestionDto(q.Id, q.Text, q.SortOrder)).ToList());
    }

    [HttpPost("{surveyId:int}/questions")]
    public async Task<IActionResult> CreateQuestion(int surveyId, CreateQuestionDto newQuestion)
    {
        var survey = await surveys.GetByIdAsync(surveyId);
        if (survey is null)
        {
            return NotFound();
        }

        var sortOrder = newQuestion.SortOrder ?? await questions.GetMaxSortOrderAsync(surveyId) + 1 ?? 1;

        var question = new Question
        {
            SurveyId = surveyId,
            Text = newQuestion.Text,
            SortOrder = sortOrder
        };

        await questions.AddAsync(question);

        var dto = new QuestionDto(question.Id, question.Text, question.SortOrder);
        return CreatedAtAction(nameof(GetQuestions), new { surveyId }, dto);
    }

    // Upserts on the (survey_id, rater_id, department_id) unique constraint:
    // returns the rater's existing submission for this survey/department if
    // one already exists (draft or submitted), otherwise starts a new draft.
    [HttpPost("{surveyId:int}/departments/{deptId:int}/submission")]
    public async Task<IActionResult> GetOrCreateSubmission(int surveyId, int deptId, GetOrCreateSubmissionDto request)
    {
        var raterDepartmentId = await users.GetDepartmentIdAsync(request.RaterId);
        if (raterDepartmentId is null)
        {
            return BadRequest("Rater not found.");
        }

        var isAssigned = await assignments.IsAssignedAsync(surveyId, raterDepartmentId.Value, deptId);
        if (!isAssigned)
        {
            return BadRequest("This department is not in scope for the rater in this survey.");
        }

        var submission = await submissions.GetOrCreateAsync(surveyId, request.RaterId, deptId);

        var dto = new SubmissionSummaryDto(
            submission.Id,
            submission.SurveyId,
            submission.RaterId,
            submission.DepartmentId,
            submission.CreatedAt,
            submission.SubmittedAt);

        return Ok(dto);
    }
}
