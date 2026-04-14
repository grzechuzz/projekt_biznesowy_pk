namespace PB.Modules.AttractionDefinition.Application.DTOs;

public record AttractionComponentDto(
    Guid Id,
    string Type,
    string Name,
    string Description,
    List<TagDto> Tags,
    LocationDto? Location,
    OpeningHoursDto? OpeningHours,
    SelectionRuleDto? SelectionRule,
    List<Guid>? ComponentIds,
    bool? IsComplete);
