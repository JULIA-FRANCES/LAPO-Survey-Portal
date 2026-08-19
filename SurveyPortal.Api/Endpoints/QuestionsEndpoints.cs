using Microsoft.EntityFrameworkCore;
using SurveyPortal.Api.Data;
using SurveyPortal.Api.Dtos;

namespace SurveyPortal.Api.Endpoints;

public static class QuestionsEndpoints
{
    public static void MapQuestionsEndpoints(this WebApplication app)
    {
        app.MapGet("/questions", async (SurveyPortalContext dbContext) =>
            await dbContext.Questions
                .OrderBy(q => q.DisplayOrder)
                .Select(q => new QuestionDto(q.Id, q.Text, q.DisplayOrder))
                .AsNoTracking()
                .ToListAsync());
    }
}