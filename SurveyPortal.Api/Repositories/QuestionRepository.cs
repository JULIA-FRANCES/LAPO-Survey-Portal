using Microsoft.EntityFrameworkCore;
using SurveyPortal.Api.Data;
using SurveyPortal.Api.Models;
using SurveyPortal.Api.Repositories.Interface;

namespace SurveyPortal.Api.Repositories;

public class QuestionRepository(SurveyPortalContext dbContext) : IQuestionRepository
{
    public Task<List<Question>> GetBySurveyAsync(int surveyId) =>
        dbContext.Questions.AsNoTracking()
            .Where(q => q.SurveyId == surveyId)
            .OrderBy(q => q.SortOrder)
            .ToListAsync();

    public Task<int?> GetMaxSortOrderAsync(int surveyId) =>
        dbContext.Questions.Where(q => q.SurveyId == surveyId).MaxAsync(q => (int?)q.SortOrder);

    public async Task AddAsync(Question question)
    {
        dbContext.Questions.Add(question);
        await dbContext.SaveChangesAsync();
    }
}
