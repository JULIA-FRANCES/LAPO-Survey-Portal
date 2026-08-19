using SurveyPortal.Api.Models;

namespace SurveyPortal.Api.Repositories.Interface;

public interface ISurveyRepository
{
    Task<Survey?> GetByIdAsync(int id);
    Task<List<Survey>> GetAllAsync();
    Task<Survey?> GetActiveAsync(DateOnly today);
    Task AddAsync(Survey survey);
}
