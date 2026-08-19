using SurveyPortal.Api.Models;

namespace SurveyPortal.Api.Repositories.Interface;

public interface IQuestionRepository
{
    Task<List<Question>> GetBySurveyAsync(int surveyId);
    Task<int?> GetMaxSortOrderAsync(int surveyId);
    Task AddAsync(Question question);
}
