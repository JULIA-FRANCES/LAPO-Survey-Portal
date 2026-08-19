using Microsoft.EntityFrameworkCore;
using SurveyPortal.Api.Data;
using SurveyPortal.Api.Models;
using SurveyPortal.Api.Repositories.Interface;

namespace SurveyPortal.Api.Repositories;

public class QuestionRepository(SurveyPortalContext dbContext) : IQuestionRepository
{
    public Task<List<Question>> GetBySurveyAsync(int surveyId, bool includeInactive = false)
    {
        var query = dbContext.Questions.AsNoTracking().Where(q => q.SurveyId == surveyId);

        if (!includeInactive)
        {
            query = query.Where(q => q.IsActive);
        }

        return query.OrderBy(q => q.SortOrder).ToListAsync();
    }

    public Task<int?> GetMaxSortOrderAsync(int surveyId) =>
        dbContext.Questions.Where(q => q.SurveyId == surveyId).MaxAsync(q => (int?)q.SortOrder);

    public async Task AddAsync(Question question)
    {
        dbContext.Questions.Add(question);
        await dbContext.SaveChangesAsync();
    }

    public async Task<Question?> SetActiveAsync(int surveyId, int questionId, bool isActive)
    {
        var question = await dbContext.Questions
            .FirstOrDefaultAsync(q => q.Id == questionId && q.SurveyId == surveyId);

        if (question is null)
        {
            return null;
        }

        question.IsActive = isActive;
        await dbContext.SaveChangesAsync();
        return question;
    }
}
