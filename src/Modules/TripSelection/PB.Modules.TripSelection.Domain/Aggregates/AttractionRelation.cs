using PB.Shared.Domain;
using PB.Modules.TripSelection.Domain.Enums;

namespace PB.Modules.TripSelection.Domain.Aggregates;

public class AttractionRelation : AggregateRoot
{
    public Guid SourceComponentId { get; }
    public Guid TargetComponentId { get; }
    public RelationType Type { get; }
    public string? Context { get; }
    public string? Description { get; }

    public AttractionRelation(Guid sourceComponentId, Guid targetComponentId, RelationType type, string? context = null, string? description = null)
    {
        if (sourceComponentId == Guid.Empty) throw new DomainException("SourceComponentId cannot be empty");
        if (targetComponentId == Guid.Empty) throw new DomainException("TargetComponentId cannot be empty");
        if (sourceComponentId == targetComponentId) throw new DomainException("Source and target cannot be the same");
        SourceComponentId = sourceComponentId;
        TargetComponentId = targetComponentId;
        Type = type;
        Context = context?.Trim();
        Description = description?.Trim();
    }
}
