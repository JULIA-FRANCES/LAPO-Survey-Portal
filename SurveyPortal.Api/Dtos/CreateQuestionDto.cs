namespace SurveyPortal.Api.Dtos;

// IsActive defaults to false: a newly-added question stays hidden from raters
// until it's explicitly published via PATCH .../questions/{id}/active.
public record CreateQuestionDto(string Text, int? SortOrder, bool IsActive = false);

public record SetQuestionActiveDto(bool IsActive);
