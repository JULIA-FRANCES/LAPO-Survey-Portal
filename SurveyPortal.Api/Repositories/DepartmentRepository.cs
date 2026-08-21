using Microsoft.EntityFrameworkCore;
using SurveyPortal.Api.Data;
using SurveyPortal.Api.Dtos;
using SurveyPortal.Api.Models;
using SurveyPortal.Api.Repositories.Interface;

namespace SurveyPortal.Api.Repositories;

public class DepartmentRepository(SurveyPortalContext dbContext) : IDepartmentRepository
{
    public Task<List<DepartmentDto>> GetAllAsync(int surveyId, int raterId) =>
    dbContext.Departments
        .AsNoTracking()
        .OrderBy(department => department.Name)
        .Select(department => new DepartmentDto(
            department.Id,
            department.Name,
            department.Units.Count,
            dbContext.Submissions.Any(submission =>
                submission.SurveyId == surveyId &&
                submission.RaterId == raterId &&
                submission.DepartmentId == department.Id &&
                submission.SubmittedAt != null)
        ))
        .ToListAsync();


    public Task<DepartmentDetailDto?> GetDetailAsync(int departmentId, int surveyId, int raterId) =>
        dbContext.Departments
            .Where(d => d.Id == departmentId)
            .Select(d => new DepartmentDetailDto(
                d.Id,
                d.Name,
                d.Units.Select(u => new UnitDto(
                    u.Id,
                    u.Name,
                    dbContext.UnitFeedback.Any(f =>
                        f.UnitId == u.Id &&
                        f.Submission!.SurveyId == surveyId &&
                        f.Submission!.RaterId == raterId &&
                        f.Submission!.SubmittedAt != null)
                )).ToList()
            ))
            .AsNoTracking()
            .FirstOrDefaultAsync();
}
