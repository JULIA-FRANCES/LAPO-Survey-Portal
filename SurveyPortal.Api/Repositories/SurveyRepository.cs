using Microsoft.EntityFrameworkCore;
using SurveyPortal.Api.Data;
using SurveyPortal.Api.Models;
using SurveyPortal.Api.Repositories.Interface;

namespace SurveyPortal.Api.Repositories;

public class SurveyRepository(SurveyPortalContext dbContext) : ISurveyRepository
{
    public Task<Survey?> GetByIdAsync(int id) =>
        dbContext.Surveys.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);

    public Task<List<Survey>> GetAllAsync() =>
        dbContext.Surveys.AsNoTracking().OrderByDescending(s => s.StartDate).ToListAsync();

    public Task<Survey?> GetActiveAsync(DateOnly today) =>
        dbContext.Surveys.AsNoTracking()
            .FirstOrDefaultAsync(s => today >= s.StartDate && today <= s.EndDate);

    public async Task AddAsync(Survey survey)
    {
        dbContext.Surveys.Add(survey);
        await dbContext.SaveChangesAsync();
    }
}
