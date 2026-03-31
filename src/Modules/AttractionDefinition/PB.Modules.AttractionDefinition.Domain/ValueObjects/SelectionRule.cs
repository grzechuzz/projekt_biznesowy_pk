using PB.Shared.Domain;
using PB.Modules.AttractionDefinition.Domain.Enums;

namespace PB.Modules.AttractionDefinition.Domain.ValueObjects;

public sealed class SelectionRule : ValueObject
{
    public SelectionRuleType Type { get; }
    public int? Count { get; }

    private SelectionRule(SelectionRuleType type, int? count)
    {
        Type = type;
        Count = count;
    }

    public static SelectionRule All() => new SelectionRule(SelectionRuleType.All, null);

    public static SelectionRule PickN(int n)
    {
        if (n <= 0) throw new DomainException("PickN count must be positive");
        return new SelectionRule(SelectionRuleType.PickN, n);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Type;
        yield return Count;
    }
}
