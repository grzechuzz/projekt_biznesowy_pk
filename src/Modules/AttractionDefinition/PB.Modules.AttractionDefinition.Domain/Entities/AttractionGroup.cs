namespace PB.Modules.AttractionDefinition.Domain.Entities;

using PB.Shared.Domain;

public class AttractionGroup : AttractionComponent
{
    private readonly List<AttractionComponent> _children = new();
    public IReadOnlyList<AttractionComponent> Children => _children.AsReadOnly();

    private AttractionGroup() { }

    public AttractionGroup(Guid id, string name, string description) : base(id)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Group name cannot be empty.");

        Name = name;
        Description = description;
    }

    public void AddChild(AttractionComponent child)
    {
        if (Status != AttractionStatus.Draft)
            throw new DomainException("Only draft groups can be modified.");
        _children.Add(child);
    }

    public void RemoveChild(Guid childId)
    {
        if (Status != AttractionStatus.Draft)
            throw new DomainException("Only draft groups can be modified.");
        _children.RemoveAll(c => c.Id == childId);
    }

    public override IReadOnlyList<SingleAttraction> GetAllAttractions()
    {
        return _children.SelectMany(c => c.GetAllAttractions()).ToList();
    }
}
