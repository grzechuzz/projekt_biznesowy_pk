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
        var session = new SelectionSession(dto.DestinationCity, dateRange, dto.GroupSize);
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

        var blockingIssues = new List<SelectionIssue>();
        var softIssues = new List<SelectionIssue>();

        // Check availability — hard block
        var isAvailable = await _availabilityQuery.IsAvailableAsync(dto.CatalogEntryId);
        if (!isAvailable)
            blockingIssues.Add(new SelectionIssue(IssueType.ConstraintViolation,
                $"No tickets available for '{entrySnapshot.Name}'"));

        // Validate booking constraints from the catalog entry
        foreach (var constraint in entrySnapshot.Constraints)
        {
            switch (constraint.Type.ToLower())
            {
                case "requireddaysahead":
                    var daysUntilTravel = (session.TravelDateRange.From.ToDateTime(TimeOnly.MinValue) - DateTime.UtcNow).Days;
                    if (constraint.MinValue.HasValue && daysUntilTravel < (int)constraint.MinValue.Value)
                        blockingIssues.Add(new SelectionIssue(IssueType.ConstraintViolation,
                            $"'{entrySnapshot.Name}' requires booking at least {(int)constraint.MinValue.Value} days ahead (you have {daysUntilTravel} days)"));
                    break;

                case "range":
                    if (constraint.Key == "group_size")
                    {
                        if (constraint.MinValue.HasValue && session.GroupSize < (int)constraint.MinValue.Value)
                            blockingIssues.Add(new SelectionIssue(IssueType.ConstraintViolation,
                                $"'{entrySnapshot.Name}' requires minimum group size of {(int)constraint.MinValue.Value} (your group: {session.GroupSize})"));
                        if (constraint.MaxValue.HasValue && session.GroupSize > (int)constraint.MaxValue.Value)
                            blockingIssues.Add(new SelectionIssue(IssueType.ConstraintViolation,
                                $"'{entrySnapshot.Name}' allows maximum group size of {(int)constraint.MaxValue.Value} (your group: {session.GroupSize})"));
                    }
                    break;

                case "min":
                    if (constraint.Key == "group_size" && constraint.MinValue.HasValue && session.GroupSize < (int)constraint.MinValue.Value)
                        blockingIssues.Add(new SelectionIssue(IssueType.ConstraintViolation,
                            $"'{entrySnapshot.Name}' requires minimum group size of {(int)constraint.MinValue.Value} (your group: {session.GroupSize})"));
                    break;

                case "max":
                    if (constraint.Key == "group_size" && constraint.MaxValue.HasValue && session.GroupSize > (int)constraint.MaxValue.Value)
                        blockingIssues.Add(new SelectionIssue(IssueType.ConstraintViolation,
                            $"'{entrySnapshot.Name}' allows maximum group size of {(int)constraint.MaxValue.Value} (your group: {session.GroupSize})"));
                    break;

                case "oneof":
                    // Soft warning — user picks a specific option (e.g. language) at booking time, not at trip selection
                    if (constraint.AllowedValues.Count > 0)
                        softIssues.Add(new SelectionIssue(IssueType.ConstraintViolation,
                            $"'{entrySnapshot.Name}' requires choosing: {string.Join(", ", constraint.AllowedValues)} (for '{constraint.Key}')"));
                    break;
            }
        }

        // Block on hard constraint violations before adding
        if (blockingIssues.Any())
            throw new DomainException(string.Join("; ", blockingIssues.Select(i => i.Message)));

        var issues = softIssues;

        // Get relations for this attraction (new item as source)
        var relations = (await _relationRepository.GetBySourceIdAsync(entrySnapshot.AttractionDefinitionId)).ToList();
        if (entrySnapshot.VariantId.HasValue)
            relations.AddRange(await _relationRepository.GetBySourceIdAsync(entrySnapshot.VariantId.Value));

        // Also fetch relations where existing items are the source (to check "existing excludes new")
        foreach (var existing in session.MustHaveItems)
        {
            relations.AddRange(await _relationRepository.GetBySourceIdAsync(existing.AttractionDefinitionId));
            if (existing.VariantId.HasValue)
                relations.AddRange(await _relationRepository.GetBySourceIdAsync(existing.VariantId.Value));
        }

        // Create item
        var newItem = new SelectionItem(
            dto.CatalogEntryId, entrySnapshot.Name, entrySnapshot.Tags,
            entrySnapshot.AttractionDefinitionId, entrySnapshot.VariantId);

        // Validate against existing items (relation-based warnings)
        var validationIssues = _relationValidationService.ValidateNewItem(newItem, session.MustHaveItems, relations);
        issues.AddRange(validationIssues);

        // Add to must-have
        session.AddMustHaveItem(newItem);

        // Get suggestions - relation targets are AttractionDefinition/Variant IDs,
        // so we look up catalog entries by their AttractionDefinitionId
        var suggestionDefIds = _relationValidationService.GetSuggestions(newItem, relations).ToList();
        var suggestionItems = new List<SelectionItem>();
        foreach (var suggestedDefId in suggestionDefIds)
        {
            var catalogEntries = await _catalogEntryQuery.GetByAttractionDefinitionIdAsync(suggestedDefId);
            foreach (var suggestedEntry in catalogEntries)
            {
                if (session.MustHaveItems.Any(i => i.CatalogEntryId == suggestedEntry.Id)) continue;
                var suggestedAvailable = await _availabilityQuery.IsAvailableAsync(suggestedEntry.Id);
                if (!suggestedAvailable) continue;
                suggestionItems.Add(new SelectionItem(
                    suggestedEntry.Id, suggestedEntry.Name, suggestedEntry.Tags,
                    suggestedEntry.AttractionDefinitionId, suggestedEntry.VariantId));
            }
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
            session.GroupSize,
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
