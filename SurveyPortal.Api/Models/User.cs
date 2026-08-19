namespace SurveyPortal.Api.Models;

public class User
{
    public int Id { get; set; }
    public required string StaffId { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public string? Location { get; set; }

    public int UnitId { get; set; }
    public Unit? Unit { get; set; }
}
