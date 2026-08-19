using Microsoft.EntityFrameworkCore;
using SurveyPortal.Api.Data;
using SurveyPortal.Api.Dtos;

namespace SurveyPortal.Api.Endpoints;

public static class DepartmentsEndpoints
{
    public static void MapDepartmentsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/departments");

        group.MapGet("/", async (SurveyPortalContext dbContext) =>
            await dbContext.Departments
                .Select(department => new DepartmentDto(
                    department.Id,
                    department.Name,
                    department.Units.Count))
                .AsNoTracking()
                .ToListAsync());

        group.MapGet("/{id}", async (int id, int evaluationCycleId, SurveyPortalContext dbContext) =>
        {
            var department = await dbContext.Departments
                .Where(d => d.Id == id)
                .Select(d => new DepartmentDetailDto(
                    d.Id,
                    d.Name,
                    d.Units.Select(u => new UnitDto(
                        u.Id,
                        u.Name,
                        dbContext.Feedback.Any(f =>
                            f.UnitId == u.Id &&
                            f.Submission!.EvaluationCycleId == evaluationCycleId)
                    )).ToList()
                ))
                .AsNoTracking()
                .FirstOrDefaultAsync();

            return department is null ? Results.NotFound() : Results.Ok(department);
        });
    }
}