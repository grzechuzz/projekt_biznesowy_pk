using PB.Shared.Domain;

namespace PB.Modules.Catalog.Domain.ValueObjects;

public sealed class DateRange : ValueObject
{
    public DateOnly From { get; }
    public DateOnly To { get; }

    public DateRange(DateOnly from, DateOnly to)
    {
        if (from > to) throw new DomainException("Start date must be before or equal to end date");
        From = from;
        To = to;
    }

    public bool Contains(DateOnly date) => date >= From && date <= To;
    public bool Overlaps(DateRange other) => From <= other.To && To >= other.From;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return From;
        yield return To;
    }
}
