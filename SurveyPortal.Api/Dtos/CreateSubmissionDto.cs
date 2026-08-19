namespace SurveyPortal.Api.Dtos;

public record CreateAnswerDto(int QuestionId, int Rating);

public record CreateUnitRatingDto(
    int UnitId,
    string FavourableFeedback,
    string CorrectiveFeedback,
    List<CreateAnswerDto> Answers);

public record CreateSubmissionDto(
    int SurveyId,
    int RaterId,
    int DepartmentId,
    List<CreateUnitRatingDto> UnitRatings);
