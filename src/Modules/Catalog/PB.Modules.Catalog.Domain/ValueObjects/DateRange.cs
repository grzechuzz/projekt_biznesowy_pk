namespace PB.Modules.Catalog.Domain.ValueObjects;

using PB.Shared.Domain;

public class DateRange : ValueObject
{
    public DateOnly From { get; }
    public DateOnly To { get; }

    public DateRange(DateOnly from, DateOnly to)
    {
        if (from > to)
            throw new DomainException("Start date must be before or equal to end date.");
        From = from;
        To = to;
    }

    public bool Contains(DateOnly date) => date >= From && date <= To;

    public bool Overlaps(DateOnly from, DateOnly to) => From <= to && To >= from;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return From;
        yield return To;
    }
}
