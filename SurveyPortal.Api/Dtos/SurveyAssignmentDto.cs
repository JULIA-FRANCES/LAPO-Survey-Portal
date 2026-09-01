namespace SurveyPortal.Api.Dtos;

public record AssignedDepartmentDto(
    int DepartmentId,
    string DepartmentName);

public record DepartmentAssignmentsDto(
    int DepartmentId,
    string DepartmentName,
    List<AssignedDepartmentDto> Assignment);

public record UpdateAssignmentDto(
    int RaterDepartmentId,
    int RatedDepartmentId);