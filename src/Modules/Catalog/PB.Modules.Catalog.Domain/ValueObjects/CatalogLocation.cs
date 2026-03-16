namespace PB.Modules.Catalog.Domain.ValueObjects;

using PB.Shared.Domain;

public class CatalogLocation : ValueObject
{
    public string City { get; }
    public string? Address { get; }

    public CatalogLocation(string city, string? address = null)
    {
        if (string.IsNullOrWhiteSpace(city))
            throw new DomainException("City cannot be empty.");
        City = city;
        Address = address;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return City;
        yield return Address;
    }
}
