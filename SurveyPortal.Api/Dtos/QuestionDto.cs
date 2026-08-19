namespace SurveyPortal.Api.Dtos;

public record QuestionDto(int Id, string Text, int SortOrder, bool IsActive);
