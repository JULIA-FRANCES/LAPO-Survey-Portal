namespace SurveyPortal.Api.Dtos;

public record AdminSurveyDto(
    int Id,
    string Name,
    DateOnly StartDate,
    DateOnly EndDate,
    int ResponseCount,
    double? AverageRating,
    string Status);

public record SurveyMetricsDto(
    int SurveyId,
    int ResponseCount,
    double? AverageRating);
