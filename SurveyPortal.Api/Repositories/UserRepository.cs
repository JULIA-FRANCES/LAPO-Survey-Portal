using Microsoft.EntityFrameworkCore;
using SurveyPortal.Api.Data;
using SurveyPortal.Api.Repositories.Interface;

namespace SurveyPortal.Api.Repositories;

public class UserRepository(SurveyPortalContext dbContext) : IUserRepository
{
    public Task<int?> GetDepartmentIdAsync(int userId) =>
        dbContext.Users
            .Where(u => u.Id == userId)
            .Select(u => (int?)u.Unit!.DepartmentId)
            .FirstOrDefaultAsync();
}
