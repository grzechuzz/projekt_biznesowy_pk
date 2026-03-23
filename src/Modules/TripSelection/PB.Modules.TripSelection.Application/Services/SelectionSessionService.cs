using PB.Shared.Domain;
using PB.Modules.TripSelection.Application.DTOs;
using PB.Modules.TripSelection.Application.Ports;
using PB.Modules.TripSelection.Domain.Aggregates;
using PB.Modules.TripSelection.Domain.Entities;
using PB.Modules.TripSelection.Domain.Enums;
using PB.Modules.TripSelection.Domain.Ports;
using PB.Modules.TripSelection.Domain.Services;
using PB.Modules.TripSelection.Domain.ValueObjects;

namespace PB.Modules.TripSelection.Application.Services;

public class SelectionSessionService : ISelectionSessionService
{
    private readonly ISelectionSessionRepository _sessionRepository;
    private readonly IAttractionRelationRepository _relationRepository;
    private readonly ICatalogEntryQuery _catalogEntryQuery;
    private readonly IAvailabilityQuery _availabilityQuery;
    private readonly IRelationValidationService _relationValidationService;

    public SelectionSessionService(
        ISelectionSessionRepository sessionRepository,
        IAttractionRelationRepository relationRepository,
        ICatalogEntryQuery catalogEntryQuery,
        IAvailabilityQuery availabilityQuery,
        IRelationValidationService relationValidationService)
    {
        _sessionRepository = sessionRepository;
        _relationRepository = relationRepository;
        _catalogEntryQuery = catalogEntryQuery;
        _availabilityQuery = availabilityQuery;
        _relationValidationService = relationValidationService;
    }

    public async Task<SelectionSessionDto> CreateAsync(CreateSessionDto dto)
    {
        var dateRange = new DateRange(dto.TravelFrom, dto.TravelTo);
        var session = new SelectionSession(dto.DestinationCity, dateRange);
        await _sessionRepository.AddAsync(session);
        return MapToDto(session);
    }

    public async Task<SelectionSessionDto?> GetByIdAsync(Guid id)
    {
        var session = await _sessionRepository.GetByIdAsync(id);
        return session == null ? null : MapToDto(session);
    }

    public async Task<SelectionSessionDto> AddItemAsync(Guid sessionId, AddItemToSessionDto dto)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId)
            ?? throw new DomainException($"Session {sessionId} not found");

        var entrySnapshot = await _catalogEntryQuery.GetByIdAsync(dto.CatalogEntryId)
            ?? throw new DomainException($"Catalog entry {dto.CatalogEntryId} not found");

        var issues = new List<SelectionIssue>();

        // Check availability
        var isAvailable = await _availabilityQuery.IsAvailableAsync(dto.CatalogEntryId);
        if (!isAvailable)
            issues.Add(new SelectionIssue(IssueType.ConstraintViolation,
                $"No tickets available for '{entrySnapshot.Name}'"));

        // Get relations for this attraction
        var relations = (await _relationRepository.GetBySourceIdAsync(entrySnapshot.AttractionDefinitionId)).ToList();
        if (entrySnapshot.VariantId.HasValue)
            relations.AddRange(await _relationRepository.GetBySourceIdAsync(entrySnapshot.VariantId.Value));

        // Create item
        var newItem = new SelectionItem(
            dto.CatalogEntryId, entrySnapshot.Name, entrySnapshot.Tags,
            entrySnapshot.AttractionDefinitionId, entrySnapshot.VariantId);

        // Validate against existing items
        var validationIssues = _relationValidationService.ValidateNewItem(newItem, session.MustHaveItems, relations);
        issues.AddRange(validationIssues);

        // Add to must-have
        session.AddMustHaveItem(newItem);

        // Get suggestions
        var suggestionIds = _relationValidationService.GetSuggestions(newItem, relations).ToList();
        var suggestionItems = new List<SelectionItem>();
        foreach (var suggestedId in suggestionIds)
        {
            var suggestedEntry = await _catalogEntryQuery.GetByIdAsync(suggestedId);
            if (suggestedEntry == null) continue;
            var suggestedAvailable = await _availabilityQuery.IsAvailableAsync(suggestedId);
            if (!suggestedAvailable) continue;
            suggestionItems.Add(new SelectionItem(
                suggestedEntry.Id, suggestedEntry.Name, suggestedEntry.Tags,
                suggestedEntry.AttractionDefinitionId, suggestedEntry.VariantId));
        }
        session.SetSuggestions(suggestionItems);

        // Get exclusions
        var exclusions = _relationValidationService.GetExclusions(newItem, relations);
        session.SetExcludedIds(session.ExcludedIds.Concat(exclusions));

        // Set issues
        session.SetIssues(issues);

        await _sessionRepository.UpdateAsync(session);
        return MapToDto(session);
    }

    public async Task<SelectionSessionDto> RemoveItemAsync(Guid sessionId, Guid catalogEntryId)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId)
            ?? throw new DomainException($"Session {sessionId} not found");
        session.RemoveMustHaveItem(catalogEntryId);
        await _sessionRepository.UpdateAsync(session);
        return MapToDto(session);
    }

    private static SelectionSessionDto MapToDto(SelectionSession session)
    {
        return new SelectionSessionDto(
            session.Id,
            session.DestinationCity,
            session.TravelDateRange.From,
            session.TravelDateRange.To,
            session.MustHaveItems.Select(MapItemDto).ToList(),
            session.OptionalSuggestions.Select(MapItemDto).ToList(),
            session.ExcludedIds.ToList(),
            session.Issues.Select(i => new SelectionIssueDto(i.Type.ToString(), i.Message, i.RelatedItemId)).ToList(),
            session.CreatedAt);
    }

    private static SelectionItemDto MapItemDto(SelectionItem item)
    {
        return new SelectionItemDto(
            item.Id,
            item.CatalogEntryId,
            item.Name,
            item.Tags.Select(t => new TagDto(t.Name, t.Group)).ToList(),
            item.AttractionDefinitionId,
            item.VariantId,
            item.AddedAt);
    }
}
