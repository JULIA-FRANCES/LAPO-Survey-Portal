namespace SurveyPortal.Api.Dtos;

public record AnswerDto(int QuestionId, string QuestionText, int Score);

public record UnitRatingDto(
    int UnitId,
    string UnitName,
    string FavourableFeedback,
    string CorrectiveFeedback,
    List<AnswerDto> Answers);

public record SubmissionDto(
    int Id,
    int EvaluationCycleId,
    DateTime SubmittedAt,
    List<UnitRatingDto> UnitRatings);