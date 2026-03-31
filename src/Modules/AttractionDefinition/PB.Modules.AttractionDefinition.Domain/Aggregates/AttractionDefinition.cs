using PB.Modules.AttractionDefinition.Domain.Entities;
using PB.Modules.AttractionDefinition.Domain.ValueObjects;
using PB.Shared.Domain;

namespace PB.Modules.AttractionDefinition.Domain.Aggregates;

public class AttractionDefinition : AttractionComponent
{
    private readonly List<AttractionVariant> _variants = new();
    public Location? Location { get; private set; }
    public OpeningHours? OpeningHours { get; private set; }
    public SeasonalAvailability? SeasonalAvailability { get; private set; }

    public IReadOnlyList<AttractionVariant> Variants => _variants.AsReadOnly();
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
    public void SetSeasonalAvailability(SeasonalAvailability? availability) => SeasonalAvailability = availability;

    public AttractionVariant AddVariant(string name, string description, int? durationMinutes = null)
    {
        if (_variants.Any(v => v.Name.Equals(name.Trim(), StringComparison.OrdinalIgnoreCase)))
            throw new DomainException($"Variant with name '{name}' already exists");
        var variant = new AttractionVariant(name, description, durationMinutes);
        _variants.Add(variant);
        return variant;
    }

    public void RemoveVariant(Guid variantId)
    {
        var variant = _variants.FirstOrDefault(v => v.Id == variantId)
            ?? throw new DomainException($"Variant {variantId} not found");
        _variants.Remove(variant);
    }

    public AttractionVariant GetVariant(Guid variantId)
        => _variants.FirstOrDefault(v => v.Id == variantId)
            ?? throw new DomainException($"Variant {variantId} not found");
}
