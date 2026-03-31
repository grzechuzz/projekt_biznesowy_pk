namespace PB.Modules.AttractionDefinition.Application.DTOs;

public record CreateAttractionDefinitionDto(
    string Name,
    string Description,
    List<TagDto>? Tags,
    LocationDto? Location,
    OpeningHoursDto? OpeningHours,
    SeasonalAvailabilityDto? SeasonalAvailability);

public record UpdateAttractionDefinitionDto(string Name, string Description);

public record AddVariantDto(
    string Name,
    string Description,
    List<TagDto>? AdditionalTags,
    List<ConstraintDto>? Constraints,
    int? DurationMinutes);

public record UpdateVariantDto(string Name, string Description, int? DurationMinutes);

public record CreatePackageDto(
    string Name,
    string Description,
    SelectionRuleDto SelectionRule,
    List<Guid>? ComponentIds);

public record UpdatePackageDto(string Name, string Description, SelectionRuleDto SelectionRule);
