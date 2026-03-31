using PB.Shared.Domain;
using PB.Modules.AttractionDefinition.Domain.ValueObjects;

namespace PB.Modules.AttractionDefinition.Domain.Entities;

public class AttractionVariant : Entity
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    private readonly HashSet<Tag> _additionalTags = new();
    private readonly List<Constraint> _constraints = new();
    public int? DurationMinutes { get; private set; }

    public IReadOnlySet<Tag> AdditionalTags => _additionalTags;
    public IReadOnlyList<Constraint> Constraints => _constraints.AsReadOnly();

    public AttractionVariant(string name, string description, int? durationMinutes = null)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Variant name cannot be empty");
        if (durationMinutes.HasValue && durationMinutes.Value <= 0)
            throw new DomainException("Duration must be positive");
        Name = name.Trim();
        Description = description?.Trim() ?? "";
        DurationMinutes = durationMinutes;
    }

    public void AddTag(Tag tag) => _additionalTags.Add(tag);
    public void RemoveTag(Tag tag) => _additionalTags.Remove(tag);
    public void AddConstraint(Constraint constraint) => _constraints.Add(constraint);

    public void RemoveConstraint(int index)
    {
        if (index < 0 || index >= _constraints.Count) throw new DomainException("Constraint index out of range");
        _constraints.RemoveAt(index);
    }

    public void Update(string name, string description, int? durationMinutes)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Variant name cannot be empty");
        if (durationMinutes.HasValue && durationMinutes.Value <= 0)
            throw new DomainException("Duration must be positive");
        Name = name.Trim();
        Description = description?.Trim() ?? "";
        DurationMinutes = durationMinutes;
    }
}
