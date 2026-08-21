using SurveyPortal.Api.Models;

namespace SurveyPortal.Api.Repositories.Interface;

public enum DeleteQuestionResult
{
    Deleted,
    NotFound
}

public interface IQuestionRepository
{
    // includeInactive = true is for admin/management views; raters filling
    // out a survey should only ever see the published (active) questions.
    Task<List<Question>> GetBySurveyAsync(int surveyId, bool includeInactive = false);
    Task<int?> GetMaxSortOrderAsync(int surveyId);
    Task AddAsync(Question question);
    Task<Question?> SetActiveAsync(int surveyId, int questionId, bool isActive);
    Task<DeleteQuestionResult> DeleteAsync(int surveyId, int questionId);
}
