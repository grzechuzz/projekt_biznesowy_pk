namespace PB.Modules.AttractionDefinition.Application.DTOs;

public record AttractionDefinitionDto(
    Guid Id,
    string Name,
    string Description,
    List<TagDto> Tags,
    LocationDto? Location,
    OpeningHoursDto? OpeningHours,
    SeasonalAvailabilityDto? SeasonalAvailability,
    List<AttractionVariantDto> Variants,
    bool IsComplete);
