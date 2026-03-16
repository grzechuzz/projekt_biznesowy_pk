namespace PB.Modules.AttractionDefinition.Application.DTOs;

public record AttractionComponentDto(
    Guid Id,
    string Name,
    string Description,
    string Status,
    string Type,
    string? Category,
    string? City,
    string? Address,
    double? Latitude,
    double? Longitude,
    TimeOnly? OpeningTime,
    TimeOnly? ClosingTime,
    bool? AvailableSpring,
    bool? AvailableSummer,
    bool? AvailableAutumn,
    bool? AvailableWinter,
    List<AttractionComponentDto>? Children);
