namespace SurveyPortal.Api.Dtos;

public record DepartmentDetailDto(int Id, string Name, List<UnitDto> Units);