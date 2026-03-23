using PB.Shared.Domain;

namespace PB.Modules.Catalog.Domain.ValueObjects;

public sealed class Discount : ValueObject
{
    public string Description { get; }
    public decimal? PercentOff { get; }
    public Money? AmountOff { get; }
    public string? Condition { get; }

    public Discount(string description, decimal? percentOff, Money? amountOff, string? condition)
    {
        if (string.IsNullOrWhiteSpace(description)) throw new DomainException("Discount description cannot be empty");
        if (percentOff.HasValue && (percentOff.Value < 0 || percentOff.Value > 100))
            throw new DomainException("Percent off must be between 0 and 100");
        if (percentOff == null && amountOff == null) throw new DomainException("Discount must have percent or amount");
        Description = description.Trim();
        PercentOff = percentOff;
        AmountOff = amountOff;
        Condition = condition?.Trim();
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Description;
        yield return PercentOff;
        yield return AmountOff;
        yield return Condition;
    }
}
