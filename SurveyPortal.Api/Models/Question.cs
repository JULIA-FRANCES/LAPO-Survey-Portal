namespace SurveyPortal.Api.Models;

public class Question
{
    public int Id { get; set; }
    public required string Text { get; set; }
    public int DisplayOrder { get; set; }
}