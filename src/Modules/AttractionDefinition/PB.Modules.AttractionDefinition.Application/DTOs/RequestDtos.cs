namespace PB.Modules.AttractionDefinition.Application.DTOs;

public record CreateAttractionComponentDto(
    string Type,
    string Name,
    string Description,
    List<TagDto>? Tags,
    LocationDto? Location,
    OpeningHoursDto? OpeningHours,
    SelectionRuleDto? SelectionRule,
    List<Guid>? ComponentIds);

public record UpdateAttractionComponentDto(
    string Name,
    string Description,
    List<TagDto>? Tags,
    LocationDto? Location,
    OpeningHoursDto? OpeningHours,
    SelectionRuleDto? SelectionRule,
    List<Guid>? ComponentIds);
