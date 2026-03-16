namespace PB.Modules.AttractionDefinition.Application.DTOs;

public record UpdateSingleAttractionDto(
    string Name,
    string Description,
    string Category,
    string City,
    string? Address,
    double? Latitude,
    double? Longitude,
    TimeOnly? OpeningTime,
    TimeOnly? ClosingTime,
    bool AvailableSpring = true,
    bool AvailableSummer = true,
    bool AvailableAutumn = true,
    bool AvailableWinter = true);
