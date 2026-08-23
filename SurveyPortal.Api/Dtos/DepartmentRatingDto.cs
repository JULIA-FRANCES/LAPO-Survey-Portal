namespace SurveyPortal.Api.Dtos;

public record SurveyDepartmentRatingDto(
    int DepartmentId,
    string DepartmentName,
    int SubmissionCount,
    double OverallAverageScore,
    List<UnitBreakdownDto> Units);

public record UnitBreakdownDto(
    int UnitId,
    string UnitName,
    int ResponseCount,
    double AverageScore,
    List<ScoreCountDto> ScoreBreakdown
    // List<QuestionBreakdownDto> Questions,
    // List<UnitFeedbackDto> Feedback
    );

public record QuestionBreakdownDto(
    int QuestionId,
    string QuestionText,
    int ResponseCount,
    double AverageScore,
    List<ScoreCountDto> ScoreBreakdown);

public record ScoreCountDto(
    int Score,
    int Count);

public record UnitFeedbackDto(
    string FavourableFeedback,
    string CorrectiveFeedback);
