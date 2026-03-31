using PB.Shared.Domain;
using PB.Modules.AttractionDefinition.Domain.Enums;

namespace PB.Modules.AttractionDefinition.Domain.ValueObjects;

public sealed class Constraint : ValueObject
{
    public ConstraintType Type { get; }
    public string Key { get; }
    public decimal? MinValue { get; }
    public decimal? MaxValue { get; }
    public IReadOnlyList<string> AllowedValues { get; }

    private Constraint(ConstraintType type, string key, decimal? min, decimal? max, IReadOnlyList<string> allowed)
    {
        if (string.IsNullOrWhiteSpace(key)) throw new DomainException("Constraint key cannot be empty");
        Type = type;
        Key = key.Trim().ToLower();
        MinValue = min;
        MaxValue = max;
        AllowedValues = allowed;
    }

    public static Constraint Range(string key, decimal min, decimal max)
    {
        if (min > max) throw new DomainException("Min cannot be greater than max");
        return new Constraint(ConstraintType.Range, key, min, max, Array.Empty<string>());
    }

    public static Constraint Min(string key, decimal min) =>
        new Constraint(ConstraintType.Min, key, min, null, Array.Empty<string>());

    public static Constraint Max(string key, decimal max) =>
        new Constraint(ConstraintType.Max, key, null, max, Array.Empty<string>());

    public static Constraint OneOf(string key, IEnumerable<string> values)
    {
        var list = values.ToList();
        if (!list.Any()) throw new DomainException("OneOf must have at least one value");
        return new Constraint(ConstraintType.OneOf, key, null, null, list.AsReadOnly());
    }

    public static Constraint RequiredDaysAhead(int days)
    {
        if (days < 0) throw new DomainException("Days ahead cannot be negative");
        return new Constraint(ConstraintType.RequiredDaysAhead, "booking_days_ahead", days, null, Array.Empty<string>());
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
