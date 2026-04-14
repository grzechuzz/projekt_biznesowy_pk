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
        var existingList = existingItems.ToList();
        var relationList = relations.ToList();

        // Check: new item EXCLUDES something already in selection
        foreach (var rel in relationList.Where(r => r.SourceComponentId == item.AttractionComponentId && r.Type == RelationType.Excludes))
        {
            var conflicting = existingList.FirstOrDefault(i => i.AttractionComponentId == rel.TargetComponentId);
            if (conflicting != null)
                issues.Add(new SelectionIssue(IssueType.Conflict,
                    $"'{item.Name}' conflicts with '{conflicting.Name}' already in your selection", conflicting.Id));
        }

        // Check: existing item EXCLUDES new item
        foreach (var existing in existingList)
        {
            foreach (var rel in relationList.Where(r => r.SourceComponentId == existing.AttractionComponentId && r.Type == RelationType.Excludes))
            {
                if (item.AttractionComponentId == rel.TargetComponentId)
                    issues.Add(new SelectionIssue(IssueType.Conflict,
                        $"'{existing.Name}' in your selection conflicts with '{item.Name}'", existing.Id));
            }
        }

        // Check: new item REQUIRES something not in selection
        foreach (var rel in relationList.Where(r => r.SourceComponentId == item.AttractionComponentId && r.Type == RelationType.Requires))
        {
            var required = existingList.Any(i => i.AttractionComponentId == rel.TargetComponentId);
            if (!required)
                issues.Add(new SelectionIssue(IssueType.RequirementMissing,
                    $"'{item.Name}' requires another attraction (component: {rel.TargetComponentId}) to be added first"));
        }

        return issues;
    }

    public IEnumerable<Guid> GetSuggestions(SelectionItem item, IEnumerable<AttractionRelation> relations)
    {
        return relations
            .Where(r => r.SourceComponentId == item.AttractionComponentId && r.Type == RelationType.Suggests)
            .Select(r => r.TargetComponentId)
            .Distinct();
    }

    public IEnumerable<Guid> GetExclusions(SelectionItem item, IEnumerable<AttractionRelation> relations)
    {
        return relations
            .Where(r => r.SourceComponentId == item.AttractionComponentId && r.Type == RelationType.Excludes)
            .Select(r => r.TargetComponentId)
            .Distinct();
    }
}
