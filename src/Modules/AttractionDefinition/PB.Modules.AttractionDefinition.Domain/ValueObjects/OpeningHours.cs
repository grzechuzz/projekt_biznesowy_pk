using PB.Shared.Domain;

namespace PB.Modules.AttractionDefinition.Domain.ValueObjects;

public sealed class OpeningHours : ValueObject
{
    public TimeOnly Open { get; }
    public TimeOnly Close { get; }

    public OpeningHours(TimeOnly open, TimeOnly close)
    {
        if (open >= close) throw new DomainException("Opening time must be before closing time");
        Open = open;
        Close = close;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Open;
        yield return Close;
    }
}
