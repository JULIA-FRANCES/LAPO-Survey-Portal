namespace SurveyPortal.Api.Models;

public class UnitFeedback
{
    public int Id { get; set; }

    public int SubmissionId { get; set; }
    public Submission? Submission { get; set; }

    public int UnitId { get; set; }
    public Unit? Unit { get; set; }

    public required string FavourableFeedback { get; set; }
    public required string CorrectiveFeedback { get; set; }
}
