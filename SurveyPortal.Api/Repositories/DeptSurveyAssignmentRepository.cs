using Microsoft.EntityFrameworkCore;
using SurveyPortal.Api.Data;
using SurveyPortal.Api.Models;
using SurveyPortal.Api.Repositories.Interface;

namespace SurveyPortal.Api.Repositories;

public class DeptSurveyAssignmentRepository(SurveyPortalContext dbContext) : IDeptSurveyAssignmentRepository
{
    public Task<List<Department>> GetRatedDepartmentsAsync(int surveyId, int raterDepartmentId) =>
        dbContext.DeptSurveyAssignments
            .Include(a => a.RatedDepartment!)
                .ThenInclude(d => d.Units)
            .Where(a => a.SurveyId == surveyId && a.RaterDepartmentId == raterDepartmentId)
            .Select(a => a.RatedDepartment!)
            .Distinct()
            .AsNoTracking()
            .ToListAsync();

    public Task<bool> IsAssignedAsync(int surveyId, int raterDepartmentId, int ratedDepartmentId) =>
        dbContext.DeptSurveyAssignments.AnyAsync(a =>
            a.SurveyId == surveyId &&
            a.RaterDepartmentId == raterDepartmentId &&
            a.RatedDepartmentId == ratedDepartmentId);

    public async Task AddRangeAsync(List<DeptSurveyAssignment> newAssignments)
    {
        dbContext.DeptSurveyAssignments.AddRange(newAssignments);
        await dbContext.SaveChangesAsync();
    }
}
