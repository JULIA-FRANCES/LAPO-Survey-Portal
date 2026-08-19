using Microsoft.EntityFrameworkCore;
using SurveyPortal.Api.Data;
using SurveyPortal.Api.Dtos;
using SurveyPortal.Api.Models;

namespace SurveyPortal.Api.Endpoints;

public static class SubmissionsEndpoints
{
    public static void MapSubmissionsEndpoints(this WebApplication app)
    {
        app.MapPost("/submissions", async (CreateSubmissionDto newSubmission, SurveyPortalContext dbContext) =>
        {
            var submission = new Submission
            {
                EvaluationCycleId = newSubmission.EvaluationCycleId,
                SubmittedAt = DateTime.UtcNow
            };

            foreach (var unitRating in newSubmission.UnitRatings)
            {
                foreach (var answer in unitRating.Answers)
                {
                    submission.Answers.Add(new Answer
                    {
                        UnitId = unitRating.UnitId,
                        QuestionId = answer.QuestionId,
                        Score = answer.Score
                    });
                }

                submission.Feedback.Add(new Feedback
                {
                    UnitId = unitRating.UnitId,
                    FavourableFeedback = unitRating.FavourableFeedback,
                    CorrectiveFeedback = unitRating.CorrectiveFeedback
                });
            }

            dbContext.Submissions.Add(submission);
            await dbContext.SaveChangesAsync();

            return Results.Created($"/submissions/{submission.Id}", submission.Id);
        });

        app.MapGet("/submissions", async (SurveyPortalContext dbContext) =>
{
    var submissions = await dbContext.Submissions
        .Include(s => s.Feedback)
            .ThenInclude(f => f.Unit)
        .Include(s => s.Answers)
            .ThenInclude(a => a.Question)
        .OrderByDescending(s => s.SubmittedAt)
        .AsNoTracking()
        .ToListAsync();

    var result = submissions.Select(s => new SubmissionDto(
        s.Id,
        s.EvaluationCycleId,
        s.SubmittedAt,
        s.Feedback.Select(f => new UnitRatingDto(
            f.UnitId,
            f.Unit!.Name,
            f.FavourableFeedback,
            f.CorrectiveFeedback,
            s.Answers
                .Where(a => a.UnitId == f.UnitId)
                .OrderBy(a => a.Question!.DisplayOrder)
                .Select(a => new AnswerDto(a.QuestionId, a.Question!.Text, a.Score))
                .ToList()
        )).ToList()
    ));

    return result;
});

    
}
    }
