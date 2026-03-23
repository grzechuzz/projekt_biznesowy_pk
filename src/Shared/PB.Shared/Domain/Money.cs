namespace PB.Shared.Domain;

public sealed class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    public static Money Free => new Money(0m, "PLN");

    public Money(decimal amount, string currency)
    {
        if (amount < 0) throw new DomainException("Amount cannot be negative");
        if (string.IsNullOrWhiteSpace(currency)) throw new DomainException("Currency cannot be empty");
        Amount = amount;
        Currency = currency.Trim().ToUpper();
    }

    public bool IsFree => Amount == 0;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public override string ToString() => $"{Amount} {Currency}";
}
