namespace PB.Modules.AttractionDefinition.Domain.Entities;

using PB.Shared.Domain;

public abstract class AttractionComponent : AggregateRoot
{
    public string Name { get; protected set; } = string.Empty;
    public string Description { get; protected set; } = string.Empty;
    public AttractionStatus Status { get; protected set; } = AttractionStatus.Draft;

    protected AttractionComponent(Guid id) : base(id) { }
    protected AttractionComponent() { }

    public abstract IReadOnlyList<SingleAttraction> GetAllAttractions();

    public void Publish()
    {
        if (Status != AttractionStatus.Draft)
            throw new DomainException("Only drafts can be published.");
        Status = AttractionStatus.Published;
    }

    public void Archive()
    {
        if (Status != AttractionStatus.Published)
            throw new DomainException("Only published attractions can be archived.");
        Status = AttractionStatus.Archived;
    }
}
