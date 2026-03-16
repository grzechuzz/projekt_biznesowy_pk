namespace PB.Modules.AttractionDefinition.Application.DTOs;

public record CreateAttractionGroupDto(
    string Name,
    string Description,
    List<Guid> ChildIds);
