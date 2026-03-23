using PB.Shared.Domain;
using PB.Modules.AttractionDefinition.Domain.Enums;

namespace PB.Modules.AttractionDefinition.Domain.ValueObjects;

public sealed class SeasonalAvailability : ValueObject
{
    public bool Spring { get; }
    public bool Summer { get; }
    public bool Autumn { get; }
    public bool Winter { get; }

    public SeasonalAvailability(bool spring, bool summer, bool autumn, bool winter)
    {
        if (!spring && !summer && !autumn && !winter)
            throw new DomainException("At least one season must be available");
        Spring = spring;
        Summer = summer;
        Autumn = autumn;
        Winter = winter;
    }

    public static SeasonalAvailability AllYear() => new SeasonalAvailability(true, true, true, true);

    public bool IsAvailableIn(Season season) => season switch
    {
        Season.Spring => Spring,
        Season.Summer => Summer,
        Season.Autumn => Autumn,
        Season.Winter => Winter,
        _ => false
    };

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Spring;
        yield return Summer;
        yield return Autumn;
        yield return Winter;
    }
}
