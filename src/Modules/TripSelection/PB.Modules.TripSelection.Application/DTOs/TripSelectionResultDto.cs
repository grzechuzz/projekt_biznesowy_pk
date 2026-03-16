namespace PB.Modules.TripSelection.Application.DTOs;

public record TripSelectionResultDto(
    Guid Id,
    Guid PreferenceId,
    string DestinationCity,
    DateOnly StartDate,
    DateOnly EndDate,
    List<SelectedAttractionDto> MustHave,
    List<SelectedAttractionDto> Optional);

public record SelectedAttractionDto(
    Guid Id,
    Guid CatalogEntryId,
    string Name,
    string Description,
    string Category,
    string City,
    string? Address,
    bool IsEvent,
    double MatchScore);
