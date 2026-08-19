namespace SurveyPortal.Api.Models;

public class DeptSurveyAssignment
{
    public int Id { get; set; }

    public int SurveyId { get; set; }
    public Survey? Survey { get; set; }

    public int RaterDepartmentId { get; set; }
    public Department? RaterDepartment { get; set; }

    public int RatedDepartmentId { get; set; }
    public Department? RatedDepartment { get; set; }
}
