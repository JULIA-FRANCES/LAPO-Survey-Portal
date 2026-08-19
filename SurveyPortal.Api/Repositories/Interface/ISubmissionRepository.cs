using SurveyPortal.Api.Models;

namespace SurveyPortal.Api.Repositories.Interface;

public interface ISubmissionRepository
{
    Task<bool> ExistsAsync(int surveyId, int raterId, int departmentId);

    // Finds the existing submission for this rater/department/survey if there
    // is one, otherwise creates and saves a fresh in-progress one.
    Task<Submission> GetOrCreateAsync(int surveyId, int raterId, int departmentId);

    // Saves a fully-built submission (answers + feedback already attached).
    Task AddAsync(Submission submission);

    Task<List<Submission>> GetAllWithDetailsAsync();
}
