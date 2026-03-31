using PB.Modules.TripSelection.Domain.Aggregates;
using PB.Modules.TripSelection.Domain.Entities;
using PB.Modules.TripSelection.Domain.ValueObjects;

namespace PB.Modules.TripSelection.Domain.Services;

public interface IRelationValidationService
{
    IEnumerable<SelectionIssue> ValidateNewItem(SelectionItem item, IEnumerable<SelectionItem> existingItems, IEnumerable<AttractionRelation> relations);
    IEnumerable<Guid> GetSuggestions(SelectionItem item, IEnumerable<AttractionRelation> relations);
    IEnumerable<Guid> GetExclusions(SelectionItem item, IEnumerable<AttractionRelation> relations);
}
