namespace SurveyPortal.Api.Dtos;

public record CreateAnswerDto(int QuestionId, int Score);

public record CreateUnitRatingDto(
    int UnitId,
    string FavourableFeedback,
    string CorrectiveFeedback,
    List<CreateAnswerDto> Answers);

public record CreateSubmissionDto(int EvaluationCycleId, List<CreateUnitRatingDto> UnitRatings);