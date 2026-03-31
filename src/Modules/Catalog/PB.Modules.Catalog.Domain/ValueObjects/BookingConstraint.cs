using PB.Shared.Domain;

namespace PB.Modules.Catalog.Domain.ValueObjects;

public sealed class BookingConstraint : ValueObject
{
    public string Type { get; }
    public string Key { get; }
    public decimal? MinValue { get; }
    public decimal? MaxValue { get; }
    public IReadOnlyList<string> AllowedValues { get; }

    public BookingConstraint(string type, string key, decimal? minValue, decimal? maxValue, IReadOnlyList<string>? allowedValues)
    {
        if (string.IsNullOrWhiteSpace(type)) throw new DomainException("Constraint type cannot be empty");
        if (string.IsNullOrWhiteSpace(key)) throw new DomainException("Constraint key cannot be empty");
        Type = type.Trim();
        Key = key.Trim().ToLower();
        MinValue = minValue;
        MaxValue = maxValue;
        AllowedValues = allowedValues ?? Array.Empty<string>();
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Type;
        yield return Key;
        yield return MinValue;
        yield return MaxValue;
        yield return string.Join(",", AllowedValues);
    }
}
