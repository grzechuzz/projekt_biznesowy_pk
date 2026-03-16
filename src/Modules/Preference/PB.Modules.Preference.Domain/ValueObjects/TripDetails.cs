namespace PB.Modules.Preference.Domain.ValueObjects;

using PB.Shared.Domain;

public class TripDetails : ValueObject
{
    public string DestinationCity { get; }
    public DateOnly StartDate { get; }
    public DateOnly EndDate { get; }

    public TripDetails(string destinationCity, DateOnly startDate, DateOnly endDate)
    {
        if (string.IsNullOrWhiteSpace(destinationCity))
            throw new DomainException("Destination city cannot be empty.");
        if (startDate > endDate)
            throw new DomainException("Start date must be before or equal to end date.");
        DestinationCity = destinationCity;
        StartDate = startDate;
        EndDate = endDate;
    }

    public int TripDurationDays => EndDate.DayNumber - StartDate.DayNumber + 1;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return DestinationCity;
        yield return StartDate;
        yield return EndDate;
    }
}
