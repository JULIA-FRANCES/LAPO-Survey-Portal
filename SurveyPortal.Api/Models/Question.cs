namespace SurveyPortal.Api.Models;

public class Question
{
    public int Id { get; set; }

    public int SurveyId { get; set; }
    public Survey? Survey { get; set; }

    public required string Text { get; set; }
    public int SortOrder { get; set; }

    // Lets a question be added ahead of time without showing up to raters
    // until someone explicitly publishes it.
    public bool IsActive { get; set; }
}
