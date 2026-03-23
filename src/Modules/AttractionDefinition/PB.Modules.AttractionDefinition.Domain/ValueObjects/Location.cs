using PB.Shared.Domain;

namespace PB.Modules.AttractionDefinition.Domain.ValueObjects;

public sealed class Location : ValueObject
{
    public string City { get; }
    public string? Address { get; }
    public double? Latitude { get; }
    public double? Longitude { get; }

    public Location(string city, string? address = null, double? latitude = null, double? longitude = null)
    {
        if (string.IsNullOrWhiteSpace(city)) throw new DomainException("City cannot be empty");
        City = city.Trim();
        Address = address?.Trim();
        Latitude = latitude;
        Longitude = longitude;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return City;
        yield return Address;
        yield return Latitude;
        yield return Longitude;
    }
}
