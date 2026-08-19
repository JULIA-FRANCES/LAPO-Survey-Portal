namespace SurveyPortal.Api.Models;

public class Survey
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }

    public List<Question> Questions { get; set; } = [];
    public List<DeptSurveyAssignment> Assignments { get; set; } = [];
}
