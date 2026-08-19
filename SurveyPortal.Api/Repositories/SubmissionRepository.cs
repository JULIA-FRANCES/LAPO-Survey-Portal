using Microsoft.EntityFrameworkCore;
using SurveyPortal.Api.Data;
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
            .Include(s => s.UnitFeedback)
                .ThenInclude(f => f.Unit)
            .Include(s => s.Answers)
                .ThenInclude(a => a.Question)
            .OrderByDescending(s => s.CreatedAt)
            .AsNoTracking()
            .ToListAsync();
}
