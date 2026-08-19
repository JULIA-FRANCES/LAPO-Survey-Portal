namespace SurveyPortal.Api.Repositories.Interface;

public interface IUserRepository
{
    // Null means the user doesn't exist. A user always belongs to a unit,
    // which belongs to a department, so this is how a rater's department is derived.
    Task<int?> GetDepartmentIdAsync(int userId);
}
