namespace PB.Modules.TripSelection.Application.DTOs;

public record TagDto(string Name, string? Group);

public record SelectionItemDto(
    Guid Id,
    Guid CatalogEntryId,
    string Name,
    List<TagDto> Tags,
    Guid AttractionComponentId,
    DateTime AddedAt);

public record SelectionIssueDto(string Type, string Message, Guid? RelatedItemId);

public record SelectionSessionDto(
    Guid Id,
    string DestinationCity,
    DateOnly TravelFrom,
    DateOnly TravelTo,
    int GroupSize,
    List<SelectionItemDto> MustHaveItems,
    List<SelectionItemDto> OptionalSuggestions,
    List<Guid> ExcludedIds,
    List<SelectionIssueDto> Issues,
    DateTime CreatedAt);

public record AttractionRelationDto(
    Guid Id,
    Guid SourceComponentId,
    Guid TargetComponentId,
    string Type,
    string? Context,
    string? Description);

public record CreateSessionDto(string DestinationCity, DateOnly TravelFrom, DateOnly TravelTo, int GroupSize = 1);

public record AddItemToSessionDto(Guid CatalogEntryId);

public record CreateRelationDto(Guid SourceComponentId, Guid TargetComponentId, string Type, string? Context, string? Description);
