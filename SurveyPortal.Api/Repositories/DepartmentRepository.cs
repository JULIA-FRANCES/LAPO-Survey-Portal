using Microsoft.EntityFrameworkCore;
using SurveyPortal.Api.Data;
using SurveyPortal.Api.Dtos;
using SurveyPortal.Api.Models;
using SurveyPortal.Api.Repositories.Interface;

namespace SurveyPortal.Api.Repositories;

public class DepartmentRepository(SurveyPortalContext dbContext) : IDepartmentRepository
{
    public Task<List<Department>> GetAllAsync() =>
        dbContext.Departments.Include(d => d.Units).AsNoTracking().ToListAsync();

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
