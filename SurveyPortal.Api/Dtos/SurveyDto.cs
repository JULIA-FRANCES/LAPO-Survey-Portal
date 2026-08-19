namespace SurveyPortal.Api.Dtos;

public record SurveyDto(int Id, string Name, DateOnly StartDate, DateOnly EndDate, string Status);
