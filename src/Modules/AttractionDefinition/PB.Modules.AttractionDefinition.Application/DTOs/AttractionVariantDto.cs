namespace PB.Modules.AttractionDefinition.Application.DTOs;

public record AttractionVariantDto(
    Guid Id,
    string Name,
    string Description,
    List<TagDto> AdditionalTags,
    List<ConstraintDto> Constraints,
    int? DurationMinutes);
