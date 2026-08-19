namespace SurveyPortal.Api.Models;

public class Department
{
    public int Id { get; set; }
    public required string Name { get; set; }

    public List<Unit> Units { get; set; } = [];
}