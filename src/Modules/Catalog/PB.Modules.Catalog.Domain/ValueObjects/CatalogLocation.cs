using PB.Shared.Domain;

namespace PB.Modules.Catalog.Domain.ValueObjects;

public sealed class CatalogLocation : ValueObject
{
    public string City { get; }
    public string? Address { get; }

    public CatalogLocation(string city, string? address = null)
    {
        if (string.IsNullOrWhiteSpace(city)) throw new DomainException("City cannot be empty");
        City = city.Trim();
        Address = address?.Trim();
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return City;
        yield return Address;
    }
}
