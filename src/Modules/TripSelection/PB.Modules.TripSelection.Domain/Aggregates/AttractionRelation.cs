using PB.Shared.Domain;
using PB.Modules.TripSelection.Domain.Enums;

namespace PB.Modules.TripSelection.Domain.Aggregates;

public class AttractionRelation : AggregateRoot
{
    public Guid SourceId { get; }
    public Guid TargetId { get; }
    public RelationType Type { get; }
    public string? Context { get; }
    public string? Description { get; }

    public AttractionRelation(Guid sourceId, Guid targetId, RelationType type, string? context = null, string? description = null)
    {
        if (sourceId == Guid.Empty) throw new DomainException("SourceId cannot be empty");
        if (targetId == Guid.Empty) throw new DomainException("TargetId cannot be empty");
        if (sourceId == targetId) throw new DomainException("Source and target cannot be the same");
        SourceId = sourceId;
        TargetId = targetId;
        Type = type;
        Context = context?.Trim();
        Description = description?.Trim();
    }
}
