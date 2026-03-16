namespace PB.Modules.Preference.Domain.ValueObjects;

using PB.Shared.Domain;

public class TransportPreference : ValueObject
{
    public bool Walking { get; }
    public bool Bicycle { get; }
    public bool PublicTransport { get; }
    public bool Car { get; }

    public TransportPreference(bool walking, bool bicycle, bool publicTransport, bool car)
    {
        if (!walking && !bicycle && !publicTransport && !car)
            throw new DomainException("At least one transport option must be selected.");
        Walking = walking;
        Bicycle = bicycle;
        PublicTransport = publicTransport;
        Car = car;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Walking;
        yield return Bicycle;
        yield return PublicTransport;
        yield return Car;
    }
}
