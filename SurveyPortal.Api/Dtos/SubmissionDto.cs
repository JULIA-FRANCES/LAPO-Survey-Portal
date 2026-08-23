namespace SurveyPortal.Api.Dtos;

public record AnswerDto(int QuestionId, string QuestionText, int Rating);

public record UnitRatingDto(
    int UnitId,
    string UnitName,
    string FavourableFeedback,
    string CorrectiveFeedback,
    List<AnswerDto> Answers);

public record SubmissionDto(
    int Id,
    int SurveyId,
    int RaterId,
    string RaterName,
    string RaterUnitName,
    string RaterDepartmentName,
    int DepartmentId,
    string DepartmentName,
    DateTime CreatedAt,
    DateTime? SubmittedAt,
    List<UnitRatingDto> UnitRatings);
