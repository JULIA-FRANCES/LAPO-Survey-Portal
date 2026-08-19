using SurveyPortal.Api.Models;

namespace SurveyPortal.Api.Repositories.Interface;

public interface IDeptSurveyAssignmentRepository
{
    // Departments in scope for a rater department: distinct rated departments
    // assigned to it for this survey.
    Task<List<Department>> GetRatedDepartmentsAsync(int surveyId, int raterDepartmentId);
    Task<bool> IsAssignedAsync(int surveyId, int raterDepartmentId, int ratedDepartmentId);
}
