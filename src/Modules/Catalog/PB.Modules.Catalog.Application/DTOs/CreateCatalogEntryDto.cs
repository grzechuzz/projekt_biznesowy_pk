namespace PB.Modules.Catalog.Application.DTOs;

public record CreateCatalogEntryDto(
    Guid AttractionDefinitionId,
    string Name,
    string Description,
    string Category,
    string City,
    string? Address,
    DateOnly ValidFrom,
    DateOnly ValidTo,
    TimeOnly? OpeningTime,
    TimeOnly? ClosingTime,
    bool IsEvent);
