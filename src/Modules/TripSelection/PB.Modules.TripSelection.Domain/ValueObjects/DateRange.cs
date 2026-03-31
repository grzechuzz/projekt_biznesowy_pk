using PB.Shared.Domain;

namespace PB.Modules.TripSelection.Domain.ValueObjects;

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

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return From;
        yield return To;
    }
}
