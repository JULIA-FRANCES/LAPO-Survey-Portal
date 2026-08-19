namespace SurveyPortal.Api.Models;

public class CompletionRecord
{
    public int Id { get; set; }

    public int StaffId { get; set; }
    public Staff? Staff { get; set; }

    public int EvaluationCycleId { get; set; }
    public EvaluationCycle? EvaluationCycle { get; set; }

    public int UnitId { get; set; }
    public Unit? Unit { get; set; }

    public DateTime CompletedAt { get; set; }
}