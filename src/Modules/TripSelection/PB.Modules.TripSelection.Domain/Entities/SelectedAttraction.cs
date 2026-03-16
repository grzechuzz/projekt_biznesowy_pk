namespace PB.Modules.TripSelection.Domain.Entities;

using PB.Shared.Domain;

public class SelectedAttraction : Entity
{
    public Guid CatalogEntryId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Category { get; private set; } = string.Empty;
    public string City { get; private set; } = string.Empty;
    public string? Address { get; private set; }
    public bool IsEvent { get; private set; }
    public double MatchScore { get; private set; }

    private SelectedAttraction() { }

    public SelectedAttraction(
        Guid id,
        Guid catalogEntryId,
        string name,
        string description,
        string category,
        string city,
        string? address,
        bool isEvent,
        double matchScore) : base(id)
    {
        CatalogEntryId = catalogEntryId;
        Name = name;
        Description = description;
        Category = category;
        City = city;
        Address = address;
        IsEvent = isEvent;
        MatchScore = matchScore;
    }
}
