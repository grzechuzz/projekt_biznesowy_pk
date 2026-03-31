using PB.Shared.Domain;
using PB.Modules.TripSelection.Domain.Entities;
using PB.Modules.TripSelection.Domain.ValueObjects;

namespace PB.Modules.TripSelection.Domain.Aggregates;

public class SelectionSession : AggregateRoot
{
    public string DestinationCity { get; }
    public DateRange TravelDateRange { get; }
    public int GroupSize { get; }
    private readonly List<SelectionItem> _mustHaveItems = new();
    private readonly List<SelectionItem> _optionalSuggestions = new();
    private readonly HashSet<Guid> _excludedIds = new();
    private readonly List<SelectionIssue> _issues = new();
    public DateTime CreatedAt { get; }

    public IReadOnlyList<SelectionItem> MustHaveItems => _mustHaveItems.AsReadOnly();
    public IReadOnlyList<SelectionItem> OptionalSuggestions => _optionalSuggestions.AsReadOnly();
    public IReadOnlySet<Guid> ExcludedIds => _excludedIds;
    public IReadOnlyList<SelectionIssue> Issues => _issues.AsReadOnly();

    public SelectionSession(string destinationCity, DateRange travelDateRange, int groupSize = 1)
    {
        if (string.IsNullOrWhiteSpace(destinationCity)) throw new DomainException("Destination city cannot be empty");
        if (groupSize < 1) throw new DomainException("Group size must be at least 1");
        DestinationCity = destinationCity.Trim();
        TravelDateRange = travelDateRange ?? throw new DomainException("Travel date range cannot be null");
        GroupSize = groupSize;
        CreatedAt = DateTime.UtcNow;
    }

    public void AddMustHaveItem(SelectionItem item)
    {
        if (_mustHaveItems.Any(i => i.CatalogEntryId == item.CatalogEntryId))
            throw new DomainException("Item already in must-have list");
        _mustHaveItems.Add(item);
        _optionalSuggestions.RemoveAll(s => s.CatalogEntryId == item.CatalogEntryId);
        _excludedIds.Remove(item.CatalogEntryId);
    }

    public void RemoveMustHaveItem(Guid catalogEntryId)
    {
        var item = _mustHaveItems.FirstOrDefault(i => i.CatalogEntryId == catalogEntryId)
            ?? throw new DomainException("Item not found in must-have list");
        _mustHaveItems.Remove(item);
    }

    public void SetSuggestions(IEnumerable<SelectionItem> suggestions)
    {
        _optionalSuggestions.Clear();
        foreach (var s in suggestions)
        {
            if (!_mustHaveItems.Any(i => i.CatalogEntryId == s.CatalogEntryId)
                && !_excludedIds.Contains(s.CatalogEntryId))
                _optionalSuggestions.Add(s);
        }
    }

    public void SetExcludedIds(IEnumerable<Guid> excludedIds)
    {
        _excludedIds.Clear();
        foreach (var id in excludedIds)
            if (!_mustHaveItems.Any(i => i.CatalogEntryId == id))
                _excludedIds.Add(id);
    }

    public void SetIssues(IEnumerable<SelectionIssue> issues)
    {
        _issues.Clear();
        _issues.AddRange(issues);
    }
}
