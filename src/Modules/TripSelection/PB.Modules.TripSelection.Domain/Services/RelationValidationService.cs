using PB.Modules.TripSelection.Domain.Aggregates;
using PB.Modules.TripSelection.Domain.Entities;
using PB.Modules.TripSelection.Domain.Enums;
using PB.Modules.TripSelection.Domain.ValueObjects;

namespace PB.Modules.TripSelection.Domain.Services;

public class RelationValidationService : IRelationValidationService
{
    public IEnumerable<SelectionIssue> ValidateNewItem(SelectionItem item, IEnumerable<SelectionItem> existingItems, IEnumerable<AttractionRelation> relations)
    {
        var issues = new List<SelectionIssue>();
        var itemIds = new HashSet<Guid> { item.AttractionDefinitionId };
        if (item.VariantId.HasValue) itemIds.Add(item.VariantId.Value);

        var existingList = existingItems.ToList();
        var relationList = relations.ToList();

        // Check: new item EXCLUDES something already in selection
        foreach (var rel in relationList.Where(r => itemIds.Contains(r.SourceId) && r.Type == RelationType.Excludes))
        {
            var conflicting = existingList.FirstOrDefault(i =>
                i.AttractionDefinitionId == rel.TargetId || i.VariantId == rel.TargetId);
            if (conflicting != null)
                issues.Add(new SelectionIssue(IssueType.Conflict,
                    $"'{item.Name}' conflicts with '{conflicting.Name}' already in your selection", conflicting.Id));
        }

        // Check: existing item EXCLUDES new item
        foreach (var existing in existingList)
        {
            var existingIds = new HashSet<Guid> { existing.AttractionDefinitionId };
            if (existing.VariantId.HasValue) existingIds.Add(existing.VariantId.Value);
            foreach (var rel in relationList.Where(r => existingIds.Contains(r.SourceId) && r.Type == RelationType.Excludes))
            {
                if (itemIds.Contains(rel.TargetId))
                    issues.Add(new SelectionIssue(IssueType.Conflict,
                        $"'{existing.Name}' in your selection conflicts with '{item.Name}'", existing.Id));
            }
        }

        // Check: new item REQUIRES something not in selection
        foreach (var rel in relationList.Where(r => itemIds.Contains(r.SourceId) && r.Type == RelationType.Requires))
        {
            var required = existingList.Any(i => i.AttractionDefinitionId == rel.TargetId || i.VariantId == rel.TargetId);
            if (!required)
                issues.Add(new SelectionIssue(IssueType.RequirementMissing,
                    $"'{item.Name}' requires another attraction (id: {rel.TargetId}) to be added first"));
        }

        return issues;
    }

    public IEnumerable<Guid> GetSuggestions(SelectionItem item, IEnumerable<AttractionRelation> relations)
    {
        var itemIds = new HashSet<Guid> { item.AttractionDefinitionId };
        if (item.VariantId.HasValue) itemIds.Add(item.VariantId.Value);
        return relations
            .Where(r => itemIds.Contains(r.SourceId) && r.Type == RelationType.Suggests)
            .Select(r => r.TargetId)
            .Distinct();
    }

    public IEnumerable<Guid> GetExclusions(SelectionItem item, IEnumerable<AttractionRelation> relations)
    {
        var itemIds = new HashSet<Guid> { item.AttractionDefinitionId };
        if (item.VariantId.HasValue) itemIds.Add(item.VariantId.Value);
        return relations
            .Where(r => itemIds.Contains(r.SourceId) && r.Type == RelationType.Excludes)
            .Select(r => r.TargetId)
            .Distinct();
    }
}
