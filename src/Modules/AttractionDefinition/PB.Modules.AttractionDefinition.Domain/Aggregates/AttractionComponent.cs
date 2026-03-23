using PB.Shared.Domain;

namespace PB.Modules.AttractionDefinition.Domain.Aggregates;

public abstract class AttractionComponent : AggregateRoot
{
    public string Name { get; protected set; }
    public string Description { get; protected set; }
    private readonly HashSet<Tag> _tags = new();

    public IReadOnlySet<Tag> Tags => _tags;

    protected AttractionComponent(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Name cannot be empty");
        Name = name.Trim();
        Description = description?.Trim() ?? "";
    }

    public void AddTag(Tag tag) => _tags.Add(tag);
    public void RemoveTag(Tag tag) => _tags.Remove(tag);
}
