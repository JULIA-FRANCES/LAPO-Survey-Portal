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
    IDepartmentRepository departments,
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

        var allDepartments = await departments.GetAllEntitiesAsync();
        var newAssignments = allDepartments
            .SelectMany(rater => allDepartments
                .Where(rated => rated.Id != rater.Id)
                .Select(rated => new DeptSurveyAssignment
                {
                    SurveyId = survey.Id,
                    RaterDepartmentId = rater.Id,
                    RatedDepartmentId = rated.Id
                }))
            .ToList();

        await assignments.AddRangeAsync(newAssignments);

        return CreatedAtAction(nameof(GetById), new { id = survey.Id }, ToDto(survey));
    }

    [HttpGet]
    public async Task<ActionResult<List<SurveyDto>>> GetAll()
    {
        var all = await surveys.GetAllAsync();
        return Ok(all.Select(ToDto).ToList());
    }

    [HttpGet("admin")]
    public async Task<ActionResult<List<AdminSurveyDto>>> GetAllForAdmin()
    {
        var all = await surveys.GetAllAsync();
        var metrics = await submissions.GetSurveyMetricsAsync();
        var metricsBySurveyId = metrics.ToDictionary(metric => metric.SurveyId);

        var result = all
            .Select(survey =>
            {
                metricsBySurveyId.TryGetValue(survey.Id, out var metric);

                return new AdminSurveyDto(
                    survey.Id,
                    survey.Name,
                    survey.StartDate,
                    survey.EndDate,
                    metric?.ResponseCount ?? 0,
                    metric?.AverageRating,
                    ComputeStatus(survey));
            })
            .ToList();

        return Ok(result);
    }

    [HttpGet("{surveyId:int}")]
    public async Task<IActionResult> GetById(int surveyId)
    {
        var survey = await surveys.GetByIdAsync(surveyId);
        return survey is null ? NotFound() : Ok(ToDto(survey));
    }

    [HttpGet("{surveyId:int}/departments")]
    public async Task<ActionResult<List<SurveyDepartmentRatingDto>>> GetDepartments(int surveyId)
    {
        var survey = await surveys.GetByIdAsync(surveyId);
        if (survey is null)
        {
            return NotFound();
        }

        var departmentRatings = await submissions.GetDepartmentSurveysAsync(surveyId);
        return Ok(departmentRatings);
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActive()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var survey = await surveys.GetActiveAsync(today);
        return survey is null ? NotFound() : Ok(ToDto(survey));
    }

    // Raters only ever see published questions. Pass includeInactive=true for
    // an admin/management view that also shows drafts not yet visible to raters.
    [HttpGet("{surveyId:int}/questions")]
    public async Task<ActionResult<List<QuestionDto>>> GetQuestions(int surveyId, bool includeInactive = false)
    {
        var list = await questions.GetBySurveyAsync(surveyId, includeInactive);
        return Ok(list.Select(q => new QuestionDto(q.Id, q.Text, q.SortOrder, q.IsActive)).ToList());
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
            SortOrder = sortOrder,
            IsActive = newQuestion.IsActive
        };

        await questions.AddAsync(question);

        var dto = new QuestionDto(question.Id, question.Text, question.SortOrder, question.IsActive);
        return CreatedAtAction(nameof(GetQuestions), new { surveyId }, dto);
    }

    // Publishes or unpublishes a question — this is how a question added ahead
    // of time (IsActive: false) becomes visible to raters.
    [HttpPatch("{surveyId:int}/questions/{questionId:int}/active")]
    public async Task<IActionResult> SetQuestionActive(int surveyId, int questionId, SetQuestionActiveDto request)
    {
        var question = await questions.SetActiveAsync(surveyId, questionId, request.IsActive);
        if (question is null)
        {
            return NotFound();
        }

        return Ok(new QuestionDto(question.Id, question.Text, question.SortOrder, question.IsActive));
    }

    [HttpDelete("{surveyId:int}/questions/{questionId:int}")]
    public async Task<IActionResult> DeleteQuestion(int surveyId, int questionId)
    {
        var result = await questions.DeleteAsync(surveyId, questionId);

        return result switch
        {
            DeleteQuestionResult.NotFound => NotFound(),
            _ => NoContent()
        };
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

    [HttpGet("{surveyId:int}/assignments")]
    public async Task<IActionResult> GetAssignmentsBySurvey(int surveyId)
    {
        var survey = await surveys.GetByIdAsync(surveyId);
        if (survey is null)
        {
            return NotFound();
        }

        var allDepartments = await departments.GetAllEntitiesAsync();
        var assignmentsForSurvey = await assignments.GetBySurveyAsync(surveyId);

        var response = allDepartments
            .OrderBy(department => department.Name)
            .Select(department => new DepartmentAssignmentsDto(
                department.Id,
                department.Name,
                allDepartments
                    .Where(assignedDepartment => assignmentsForSurvey.Any(assignment =>
                        assignment.RaterDepartmentId == department.Id &&
                        assignment.RatedDepartmentId == assignedDepartment.Id))
                    .OrderBy(assignedDepartment => assignedDepartment.Name)
                    .Select(assignedDepartment => new AssignedDepartmentDto(
                        assignedDepartment.Id,
                        assignedDepartment.Name))
                    .ToList()))
            .ToList();

        return Ok(response);
    }

    [HttpPut("{surveyId:int}/assignments")]
    public async Task<IActionResult> ReplaceAssignments(int surveyId, List<DeptSurveyAssignment> newAssignments)
    {
        var survey = await surveys.GetByIdAsync(surveyId);
        if (survey is null)
        {
            return NotFound();
        }

        await assignments.ReplaceAsync(surveyId, newAssignments);

        return NoContent();
    }
}
