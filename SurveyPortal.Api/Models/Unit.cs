namespace SurveyPortal.Api.Models;

public class Unit
{
    public int Id { get; set; }
    public required string Name { get; set; }

    public int DepartmentId { get; set; }
    public Department? Department { get; set; }
    public List<Submission> Submissions { get; set; } = [];
}