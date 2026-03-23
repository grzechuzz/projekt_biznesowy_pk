using PB.Shared.Domain;

namespace PB.Modules.Catalog.Domain.ValueObjects;

public sealed class PricingPeriod : ValueObject
{
    public DateRange DateRange { get; }
    public Money Price { get; }
    public IReadOnlyList<Discount> Discounts { get; }

    public PricingPeriod(DateRange dateRange, Money price, IEnumerable<Discount>? discounts = null)
    {
        DateRange = dateRange ?? throw new DomainException("Date range cannot be null");
        Price = price ?? throw new DomainException("Price cannot be null");
        Discounts = discounts?.ToList().AsReadOnly() ?? new List<Discount>().AsReadOnly();
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return DateRange;
        yield return Price;
    }
}
