using PB.Shared.Domain;

namespace PB.Modules.TripSelection.Domain.Entities;

public class SelectionItem : Entity
{
    public Guid CatalogEntryId { get; }
    public string Name { get; }
    public IReadOnlySet<Tag> Tags { get; }
    public Guid AttractionDefinitionId { get; }
    public Guid? VariantId { get; }
    public DateTime AddedAt { get; }

    public SelectionItem(Guid catalogEntryId, string name, IEnumerable<Tag> tags,
        Guid attractionDefinitionId, Guid? variantId)
    {
        if (catalogEntryId == Guid.Empty) throw new DomainException("CatalogEntryId cannot be empty");
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Name cannot be empty");
        CatalogEntryId = catalogEntryId;
        Name = name.Trim();
        Tags = new HashSet<Tag>(tags ?? Enumerable.Empty<Tag>());
        AttractionDefinitionId = attractionDefinitionId;
        VariantId = variantId;
        AddedAt = DateTime.UtcNow;
    }
}
