using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SurveyPortal.Api.Dtos;
using SurveyPortal.Api.Models;
using SurveyPortal.Api.Repositories.Interface;

namespace SurveyPortal.Api.Controllers;

[ApiController]
[Route("submissions")]
public class SubmissionsController(
    ISubmissionRepository submissions,
    IUserRepository users,
    IDeptSurveyAssignmentRepository assignments) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateSubmissionDto newSubmission)
    {
        var raterDepartmentId = await users.GetDepartmentIdAsync(newSubmission.RaterId);
        if (raterDepartmentId is null)
        {
            return BadRequest("Rater not found.");
        }

        var isAssigned = await assignments.IsAssignedAsync(
            newSubmission.SurveyId, raterDepartmentId.Value, newSubmission.DepartmentId);

        if (!isAssigned)
        {
            return BadRequest("This department is not in scope for the rater in this survey.");
        }

        // POST /submissions is create-only (not an upsert), so reject up front
        // if this rater already has a submission for this survey/department —
        // they should use the get-or-create endpoint to resume it instead.
        if (await submissions.ExistsAsync(newSubmission.SurveyId, newSubmission.RaterId, newSubmission.DepartmentId))
        {
            return Conflict("A submission already exists for this rater, department, and survey.");
        }

        var now = DateTime.UtcNow;
        var submission = new Submission
        {
            SurveyId = newSubmission.SurveyId,
            RaterId = newSubmission.RaterId,
            DepartmentId = newSubmission.DepartmentId,
            CreatedAt = now,
            SubmittedAt = now
        };

        foreach (var unitRating in newSubmission.UnitRatings)
        {
            foreach (var answer in unitRating.Answers)
            {
                submission.Answers.Add(new Answer
                {
                    UnitId = unitRating.UnitId,
                    QuestionId = answer.QuestionId,
                    Rating = answer.Rating
                });
            }

            submission.UnitFeedback.Add(new UnitFeedback
            {
                UnitId = unitRating.UnitId,
                FavourableFeedback = unitRating.FavourableFeedback,
                CorrectiveFeedback = unitRating.CorrectiveFeedback
            });
        }

        try
        {
            await submissions.AddAsync(submission);
        }
        catch (DbUpdateException)
        {
            // Safety net for the rare race where two requests both pass the
            // ExistsAsync check above before either one saves.
            return Conflict("A submission already exists for this rater, department, and survey.");
        }

        return CreatedAtAction(nameof(Create), new { id = submission.Id }, submission.Id);
    }

    [HttpGet]
    public async Task<ActionResult<List<SubmissionDto>>> GetAll()
    {
        var all = await submissions.GetAllWithDetailsAsync();

        var result = all.Select(s => new SubmissionDto(
            s.Id,
            s.SurveyId,
            s.RaterId,
            s.Rater.Name,
            s.Rater.Unit!.Name,
            s.Rater.Unit!.Department!.Name,
            s.DepartmentId,
            s.Department!.Name,
            s.CreatedAt,
            s.SubmittedAt,
            s.UnitFeedback.Select(f => new UnitRatingDto(
                f.UnitId,
                f.Unit!.Name,
                f.FavourableFeedback,
                f.CorrectiveFeedback,
                s.Answers
                    .Where(a => a.UnitId == f.UnitId)
                    .OrderBy(a => a.Question!.SortOrder)
                    .Select(a => new AnswerDto(a.QuestionId, a.Question!.Text, a.Rating))
                    .ToList()
            )).ToList()
        )).ToList();

        return Ok(result);
    }
}
