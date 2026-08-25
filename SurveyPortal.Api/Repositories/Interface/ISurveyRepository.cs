using SurveyPortal.Api.Models;
using SurveyPortal.Api.Dtos;

namespace SurveyPortal.Api.Repositories.Interface;

public interface ISurveyRepository
{
    Task<Survey?> GetByIdAsync(int id);
    Task<List<Survey>> GetAllAsync();
    Task<Survey?> GetActiveAsync(DateOnly today);
    Task AddAsync(Survey survey);
    Task<Survey?> UpdateAsync(int id, CreateSurveyDto survey);
}
