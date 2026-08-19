namespace SurveyPortal.Api.Models;

public class EvaluationCycle
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public bool IsActive { get; set; }
    public int MinDepartmentsRequired { get; set; } = 5;
}