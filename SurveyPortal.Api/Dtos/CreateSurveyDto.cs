namespace SurveyPortal.Api.Dtos;

public record CreateSurveyDto(string Name, DateOnly StartDate, DateOnly EndDate);
