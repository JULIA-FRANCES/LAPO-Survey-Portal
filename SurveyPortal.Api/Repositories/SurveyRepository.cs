using Microsoft.EntityFrameworkCore;
using SurveyPortal.Api.Data;
using SurveyPortal.Api.Dtos;
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

    public async Task<Survey?> UpdateAsync(int id, CreateSurveyDto survey)
    {
        var existingSurvey = await dbContext.Surveys.FindAsync(id);
        if (existingSurvey is null)
        {
            return null;
        }

        existingSurvey.Name = survey.Name;
        existingSurvey.StartDate = survey.StartDate;
        existingSurvey.EndDate = survey.EndDate;

        await dbContext.SaveChangesAsync();
        return existingSurvey;
    }
}
