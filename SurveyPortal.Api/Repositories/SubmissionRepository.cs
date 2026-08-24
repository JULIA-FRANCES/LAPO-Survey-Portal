using Microsoft.EntityFrameworkCore;
using SurveyPortal.Api.Data;
using SurveyPortal.Api.Dtos;
using SurveyPortal.Api.Models;
using SurveyPortal.Api.Repositories.Interface;

namespace SurveyPortal.Api.Repositories;

public class SubmissionRepository(SurveyPortalContext dbContext) : ISubmissionRepository
{
    public Task<bool> ExistsAsync(int surveyId, int raterId, int departmentId) =>
        dbContext.Submissions.AnyAsync(s =>
            s.SurveyId == surveyId && s.RaterId == raterId && s.DepartmentId == departmentId);

    public async Task<Submission> GetOrCreateAsync(int surveyId, int raterId, int departmentId)
    {
        var submission = await dbContext.Submissions.FirstOrDefaultAsync(s =>
            s.SurveyId == surveyId && s.RaterId == raterId && s.DepartmentId == departmentId);

        if (submission is not null)
        {
            return submission;
        }

        submission = new Submission
        {
            SurveyId = surveyId,
            RaterId = raterId,
            DepartmentId = departmentId,
            CreatedAt = DateTime.UtcNow,
            SubmittedAt = null
        };

        dbContext.Submissions.Add(submission);
        await dbContext.SaveChangesAsync();
        return submission;
    }

    public async Task AddAsync(Submission submission)
    {
        dbContext.Submissions.Add(submission);
        await dbContext.SaveChangesAsync();
    }

    public Task<List<Submission>> GetAllWithDetailsAsync() =>
        dbContext.Submissions
            .Include(s => s.Rater)
                .ThenInclude(r => r.Unit)
                    .ThenInclude(u => u.Department)
            .Include(s => s.Department)
            .Include(s => s.UnitFeedback)
                .ThenInclude(f => f.Unit)
            .Include(s => s.Answers)
                .ThenInclude(a => a.Question)
            .OrderByDescending(s => s.CreatedAt)
            .AsNoTracking()
            .ToListAsync();

    public async Task<List<SurveyDepartmentRatingDto>> GetDepartmentSurveysAsync(int surveyId)
    {
        var submissions = await dbContext.Submissions
            .AsNoTracking()
            .Where(s => s.SurveyId == surveyId && s.SubmittedAt != null)
            .Include(s => s.Department)
            .Include(s => s.Answers)
                .ThenInclude(a => a.Unit)
            // .Include(s => s.Answers)
            //     .ThenInclude(a => a.Question)
            .Include(s => s.UnitFeedback)
            .ToListAsync();

        return submissions
            .GroupBy(s => new { s.DepartmentId, s.Department!.Name })
            .Select(department => new SurveyDepartmentRatingDto(
                department.Key.DepartmentId,
                department.Key.Name,
                department.Count(),
                department.SelectMany(s => s.Answers).Average(a => (double)a.Rating),
                department.SelectMany(s => s.Answers)
                    .GroupBy(a => new { a.UnitId, a.Unit!.Name })
                    .Select(unit => new UnitBreakdownDto(
                        unit.Key.UnitId,
                        unit.Key.Name,
                        unit.Count(),
                        unit.Average(a => (double)a.Rating),
                        unit.GroupBy(a => a.Rating)
                            .Select(score => new ScoreCountDto(score.Key, score.Count()))
                            .ToList()
                            // unit.GroupBy(a => new { a.QuestionId, a.Question!.Text })
                            //     .Select(question => new QuestionBreakdownDto(
                            //         question.Key.QuestionId,
                            //         question.Key.Text,
                            //         question.Count(),
                            //         question.Average(a => (double)a.Rating),
                            //         question.GroupBy(a => a.Rating)
                            //             .Select(score => new ScoreCountDto(score.Key, score.Count()))
                            //             .ToList()))
                            //     .ToList(),
                            // department.SelectMany(s => s.UnitFeedback)
                            //     .Where(feedback => feedback.UnitId == unit.Key.UnitId)
                            //     .Select(feedback => new UnitFeedbackDto(
                            //         feedback.FavourableFeedback,
                            //         feedback.CorrectiveFeedback))
                            //     .ToList()
                            )
                            )
                    .ToList())
                    )
            .ToList();
    }

    public Task<List<SurveyMetricsDto>> GetSurveyMetricsAsync() =>
        dbContext.Surveys
            .AsNoTracking()
            .Select(survey => new SurveyMetricsDto(
                survey.Id,
                dbContext.Submissions.Count(submission =>
                    submission.SurveyId == survey.Id &&
                    submission.SubmittedAt != null),
                dbContext.Answers
                    .Where(answer =>
                        answer.Submission!.SurveyId == survey.Id &&
                        answer.Submission.SubmittedAt != null)
                    .Average(answer => (double?)answer.Rating)))
            .ToListAsync();
}
