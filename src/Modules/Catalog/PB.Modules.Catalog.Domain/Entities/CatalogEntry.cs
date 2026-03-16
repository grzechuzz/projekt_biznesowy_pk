namespace PB.Modules.Catalog.Domain.Entities;

using PB.Shared.Domain;
using PB.Modules.Catalog.Domain.ValueObjects;

public class CatalogEntry : AggregateRoot
{
    public Guid AttractionDefinitionId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Category { get; private set; } = string.Empty;
    public CatalogLocation Location { get; private set; } = null!;
    public DateRange ValidPeriod { get; private set; } = null!;
    public CatalogOpeningHours? OpeningHours { get; private set; }
    public bool IsEvent { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private CatalogEntry() { }

    public CatalogEntry(
        Guid id,
        Guid attractionDefinitionId,
        string name,
        string description,
        string category,
        CatalogLocation location,
        DateRange validPeriod,
        CatalogOpeningHours? openingHours,
        bool isEvent) : base(id)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Catalog entry name cannot be empty.");

        AttractionDefinitionId = attractionDefinitionId;
        Name = name;
        Description = description;
        Category = category;
        Location = location;
        ValidPeriod = validPeriod;
        OpeningHours = openingHours;
        IsEvent = isEvent;
        CreatedAt = DateTime.UtcNow;
    }

    public bool IsAvailableOn(DateOnly date)
    {
        return ValidPeriod.Contains(date);
    }

    public bool IsAvailableBetween(DateOnly from, DateOnly to)
    {
        return ValidPeriod.Overlaps(from, to);
    }
}
