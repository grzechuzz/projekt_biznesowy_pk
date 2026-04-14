using PB.Shared.Domain;
using PB.Modules.Catalog.Domain.Enums;
using PB.Modules.Catalog.Domain.ValueObjects;

namespace PB.Modules.Catalog.Domain.Aggregates;

public class CatalogEntry : AggregateRoot
{
    public Guid AttractionComponentId { get; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    private readonly HashSet<Tag> _tags = new();
    public CatalogLocation Location { get; private set; }
    public DateRange DateRange { get; private set; }
    public CatalogOpeningHours? OpeningHours { get; private set; }
    public bool IsEvent { get; private set; }
    public CatalogEntryStatus Status { get; private set; } = CatalogEntryStatus.Available;
    private readonly List<PricingPeriod> _pricingPeriods = new();
    private readonly List<BookingConstraint> _constraints = new();

    public IReadOnlySet<Tag> Tags => _tags;
    public IReadOnlyList<PricingPeriod> PricingPeriods => _pricingPeriods.AsReadOnly();
    public IReadOnlyList<BookingConstraint> Constraints => _constraints.AsReadOnly();

    public CatalogEntry(Guid attractionComponentId, string name, string description,
        CatalogLocation location, DateRange dateRange, bool isEvent, IEnumerable<Tag>? tags = null,
        IEnumerable<BookingConstraint>? constraints = null)
    {
        if (attractionComponentId == Guid.Empty) throw new DomainException("AttractionComponentId cannot be empty");
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Name cannot be empty");
        AttractionComponentId = attractionComponentId;
        Name = name.Trim();
        Description = description?.Trim() ?? "";
        Location = location ?? throw new DomainException("Location cannot be null");
        DateRange = dateRange ?? throw new DomainException("DateRange cannot be null");
        IsEvent = isEvent;
        if (tags != null) foreach (var tag in tags) _tags.Add(tag);
        if (constraints != null) foreach (var c in constraints) _constraints.Add(c);
    }

    public void Update(string name, string description, CatalogLocation location, DateRange dateRange, bool isEvent)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Name cannot be empty");
        if (Status == CatalogEntryStatus.Cancelled) throw new DomainException("Cannot update a cancelled entry");
        Name = name.Trim();
        Description = description?.Trim() ?? "";
        Location = location ?? throw new DomainException("Location cannot be null");
        DateRange = dateRange ?? throw new DomainException("DateRange cannot be null");
        IsEvent = isEvent;
    }

    public void AddTag(Tag tag) => _tags.Add(tag);
    public void RemoveTag(Tag tag) => _tags.Remove(tag);

    public void SetOpeningHours(CatalogOpeningHours? hours) => OpeningHours = hours;

    public void AddPricingPeriod(PricingPeriod period)
    {
        if (period == null) throw new DomainException("Pricing period cannot be null");
        var overlaps = _pricingPeriods.Any(p => p.DateRange.Overlaps(period.DateRange));
        if (overlaps) throw new DomainException("Pricing period overlaps with an existing period");
        _pricingPeriods.Add(period);
    }

    public void RemovePricingPeriodAt(int index)
    {
        if (index < 0 || index >= _pricingPeriods.Count) throw new DomainException("Pricing period index out of range");
        _pricingPeriods.RemoveAt(index);
    }

    public Money? GetPriceForDate(DateOnly date)
        => _pricingPeriods.FirstOrDefault(p => p.DateRange.Contains(date))?.Price;

    public void Cancel()
    {
        if (Status == CatalogEntryStatus.Cancelled) throw new DomainException("Entry is already cancelled");
        Status = CatalogEntryStatus.Cancelled;
    }

    public void MarkAsSoldOut()
    {
        if (Status == CatalogEntryStatus.Cancelled) throw new DomainException("Cannot mark a cancelled entry as sold out");
        Status = CatalogEntryStatus.SoldOut;
    }

    public void MarkAsAvailable()
    {
        if (Status == CatalogEntryStatus.Cancelled) throw new DomainException("Cannot restore a cancelled entry");
        Status = CatalogEntryStatus.Available;
    }
}
