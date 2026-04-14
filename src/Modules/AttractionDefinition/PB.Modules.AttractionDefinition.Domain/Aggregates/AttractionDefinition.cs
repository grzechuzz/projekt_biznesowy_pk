using PB.Modules.AttractionDefinition.Domain.ValueObjects;
using PB.Shared.Domain;

namespace PB.Modules.AttractionDefinition.Domain.Aggregates;

public class AttractionDefinition : AttractionComponent
{
    public Location? Location { get; private set; }
    public OpeningHours? OpeningHours { get; private set; }
    public bool IsComplete => !string.IsNullOrWhiteSpace(Name) && Tags.Any() && Location != null;

    public AttractionDefinition(string name, string description) : base(name, description) { }

    public void Update(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Name cannot be empty");
        Name = name.Trim();
        Description = description?.Trim() ?? "";
    }

    public void SetLocation(Location location) =>
        Location = location ?? throw new DomainException("Location cannot be null");

    public void SetOpeningHours(OpeningHours? openingHours) => OpeningHours = openingHours;
}
