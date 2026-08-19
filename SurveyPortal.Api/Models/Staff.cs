namespace SurveyPortal.Api.Models;

public class Staff
{
    public int Id { get; set; }
    public required string StaffId { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string Location { get; set; }

    public int DepartmentId { get; set; }
    public Department? Department { get; set; }

    public int UnitId { get; set; }
    public Unit? Unit { get; set; }
}