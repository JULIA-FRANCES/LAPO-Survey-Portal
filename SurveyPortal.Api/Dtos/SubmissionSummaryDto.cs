namespace SurveyPortal.Api.Dtos;

public record GetOrCreateSubmissionDto(int RaterId);

public record SubmissionSummaryDto(
    int Id,
    int SurveyId,
    int RaterId,
    int DepartmentId,
    DateTime CreatedAt,
    DateTime? SubmittedAt);
