namespace SurveyPortal.Api.Models;

public class Unit
{
    public int Id { get; set; }
    public required string Name { get; set; }

    public int DepartmentId { get; set; }
    public Department? Department { get; set; }

    public List<User> Users { get; set; } = [];
    public List<Answer> Answers { get; set; } = [];
}
