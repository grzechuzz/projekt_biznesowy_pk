namespace PB.Modules.AttractionDefinition.Application.DTOs;

public record AttractionPackageDto(
    Guid Id,
    string Name,
    string Description,
    SelectionRuleDto SelectionRule,
    List<Guid> ComponentIds);
