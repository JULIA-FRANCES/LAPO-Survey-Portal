using Microsoft.EntityFrameworkCore;
using SurveyPortal.Api.Data;
using SurveyPortal.Api.Dtos;

namespace SurveyPortal.Api.Endpoints;

public static class EvaluationCyclesEndpoints
{
    public static void MapEvaluationCyclesEndpoints(this WebApplication app)
    {
        app.MapGet("/evaluation-cycles/active", async (SurveyPortalContext dbContext) =>
        {
            var cycle = await dbContext.EvaluationCycles
                .Where(c => c.IsActive)
                .Select(c => new EvaluationCycleDto(c.Id, c.Name, c.MinDepartmentsRequired))
                .AsNoTracking()
                .FirstOrDefaultAsync();

            return cycle is null ? Results.NotFound() : Results.Ok(cycle);
        });
    }
}