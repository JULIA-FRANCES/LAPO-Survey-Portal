using SurveyPortal.Api.Dtos;
using SurveyPortal.Api.Models;

namespace SurveyPortal.Api.Repositories.Interface;

public interface IDepartmentRepository
{
    Task<List<Department>> GetAllAsync();

    // This one returns a DTO instead of a model: the "is this unit completed"
    // flag is computed per-rater and doesn't belong on the Department entity,
    // so there's no plain model this query could return instead.
    Task<DepartmentDetailDto?> GetDetailAsync(int departmentId, int surveyId, int raterId);
}
