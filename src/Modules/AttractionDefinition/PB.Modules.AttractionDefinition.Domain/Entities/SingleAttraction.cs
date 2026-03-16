namespace PB.Modules.AttractionDefinition.Domain.Entities;

using PB.Modules.AttractionDefinition.Domain.ValueObjects;
using PB.Shared.Domain;

public class SingleAttraction : AttractionComponent
{
    public Category Category { get; private set; } = null!;
    public Location Location { get; private set; } = null!;
    public OpeningHours? OpeningHours { get; private set; }
    public SeasonalAvailability SeasonalAvailability { get; private set; } = null!;

    private SingleAttraction() { }

    public SingleAttraction(
        Guid id,
        string name,
        string description,
        Category category,
        Location location,
        OpeningHours? openingHours,
        SeasonalAvailability seasonalAvailability) : base(id)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Attraction name cannot be empty.");

        Name = name;
        Description = description;
        Category = category;
        Location = location;
        OpeningHours = openingHours;
        SeasonalAvailability = seasonalAvailability;
    }

    public override IReadOnlyList<SingleAttraction> GetAllAttractions() => new List<SingleAttraction> { this };

    public void Update(string name, string description, Category category, Location location,
        OpeningHours? openingHours, SeasonalAvailability seasonalAvailability)
    {
        if (Status != AttractionStatus.Draft)
            throw new DomainException("Only drafts can be edited.");

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Attraction name cannot be empty.");

        Name = name;
        Description = description;
        Category = category;
        Location = location;
        OpeningHours = openingHours;
        SeasonalAvailability = seasonalAvailability;
    }
}
