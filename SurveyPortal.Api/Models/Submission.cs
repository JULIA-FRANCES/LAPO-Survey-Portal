namespace SurveyPortal.Api.Models;

public class Submission
{
    public int Id { get; set; }

    public int SurveyId { get; set; }
    public Survey? Survey { get; set; }

    public int RaterId { get; set; }
    public User? Rater { get; set; }

    public int DepartmentId { get; set; }
    public Department? Department { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }

    public List<Answer> Answers { get; set; } = [];
    public List<UnitFeedback> UnitFeedback { get; set; } = [];
}
