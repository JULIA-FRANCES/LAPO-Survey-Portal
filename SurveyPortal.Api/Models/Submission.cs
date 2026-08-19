namespace SurveyPortal.Api.Models;

public class Submission
{
    public int Id { get; set; }

    public int EvaluationCycleId { get; set; }
    public EvaluationCycle? EvaluationCycle { get; set; }

    public DateTime SubmittedAt { get; set; }

    public List<Answer> Answers { get; set; } = [];
    public List<Feedback> Feedback { get; set; } = [];
}