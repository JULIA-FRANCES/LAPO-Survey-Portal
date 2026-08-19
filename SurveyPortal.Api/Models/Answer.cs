namespace SurveyPortal.Api.Models;

public class Answer
{
    public int Id { get; set; }

    public int SubmissionId { get; set; }
    public Submission? Submission { get; set; }

    public int UnitId { get; set; }
    public Unit? Unit { get; set; }

    public int QuestionId { get; set; }
    public Question? Question { get; set; }

    public int Score { get; set; }
}